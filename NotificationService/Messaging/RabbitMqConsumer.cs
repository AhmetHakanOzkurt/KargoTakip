using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Messaging;
using KargoTakip.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using NotificationService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Messaging
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            EmailService emailService,
            ILogger<RabbitMqConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";

            // RabbitMQ hazır olana kadar bekle
            IConnection? connection = null;
            while (connection == null && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var factory = new ConnectionFactory { HostName = hostName };
                    connection = await factory.CreateConnectionAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "RabbitMQ baglantisi kurulamadi, 5 saniye sonra tekrar denenecek.");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            if (connection == null) return;

            _connection = connection;
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Ayni anda islenecek mesaj sayisini sinirla; aksi halde tum kuyruk
            // bellege cekiliyordu.
            await _channel.BasicQosAsync(0, 10, false, stoppingToken);

            // Islenemeyen mesajlar sonsuz requeue yerine dead-letter kuyruguna gider.
            await _channel.ExchangeDeclareAsync(
                exchange: KuyrukTopolojisi.DeadLetterExchange,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            foreach (var kuyruk in KuyrukTopolojisi.TuketilenKuyruklar)
            {
                var dlq = KuyrukTopolojisi.DlqAdi(kuyruk);

                await _channel.QueueDeclareAsync(
                    queue: dlq,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken
                );

                await _channel.QueueBindAsync(
                    queue: dlq,
                    exchange: KuyrukTopolojisi.DeadLetterExchange,
                    routingKey: dlq,
                    cancellationToken: stoppingToken
                );

                await _channel.QueueDeclareAsync(
                    queue: kuyruk,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: KuyrukTopolojisi.Argumanlar(kuyruk),
                    cancellationToken: stoppingToken
                );
            }

            // kargo_olusturuldu dinle
            var kargoOlusturulduConsumer = new AsyncEventingBasicConsumer(_channel);
            kargoOlusturulduConsumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var ev = JsonSerializer.Deserialize<KargoOlusturulduEvent>(message);
                    if (ev != null)
                        await HandleKargoOlusturuldu(ev);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    // requeue:false -> mesaj DLQ'ya gider. Onceden true idi ve
                    // bozuk bir mesaj kuyrugu sonsuz donguye sokuyordu.
                    _logger.LogError(ex,
                        "Mesaj islenemedi, DLQ'ya aktariliyor. Govde: {Body}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "kargo_olusturuldu",
                autoAck: false,
                consumer: kargoOlusturulduConsumer,
                cancellationToken: stoppingToken
            );

            // kargo_durumu_guncellendi dinle
            var kargoDurumuConsumer = new AsyncEventingBasicConsumer(_channel);
            kargoDurumuConsumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var ev = JsonSerializer.Deserialize<KargoDurumuGuncellendiEvent>(message);
                    if (ev != null)
                        await HandleKargoDurumuGuncellendi(ev);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    // requeue:false -> mesaj DLQ'ya gider. Onceden true idi ve
                    // bozuk bir mesaj kuyrugu sonsuz donguye sokuyordu.
                    _logger.LogError(ex,
                        "Mesaj islenemedi, DLQ'ya aktariliyor. Govde: {Body}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "kargo_durumu_guncellendi",
                autoAck: false,
                consumer: kargoDurumuConsumer,
                cancellationToken: stoppingToken
            );

            // Servis çalışmaya devam etsin
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task HandleKargoOlusturuldu(KargoOlusturulduEvent ev)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<KargoTakipDbContext>();

            var mesaj = $"{ev.TrackingCode} takip kodlu kargonuz hazırlanıyor.";

            // Mesaj yeniden teslim edilirse ayni bildirim ve mail tekrar
            // uretilmemeli.
            if (await context.Notifications.AnyAsync(n =>
                    n.ShipmentId == ev.ShipmentId && n.Message == mesaj))
            {
                _logger.LogInformation(
                    "Bildirim zaten mevcut, tekrar islenmedi: {TrackingCode}",
                    ev.TrackingCode);
                return;
            }

            var notification = new Notification
            {
                ShipmentId = ev.ShipmentId,
                BranchId = ev.BranchId,
                Message = mesaj,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            // Müşteriye mail gönder
            if (!string.IsNullOrEmpty(ev.ReceiverEmail))
            {
                await _emailService.SendOrderCreatedEmailAsync(
                    ev.ReceiverEmail,
                    ev.ReceiverName ?? "Değerli Müşterimiz",
                    ev.TrackingCode
                );
            }

            _logger.LogInformation("Bildirim olusturuldu: {Message}", notification.Message);
        }

        private async Task HandleKargoDurumuGuncellendi(KargoDurumuGuncellendiEvent ev)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<KargoTakipDbContext>();

            var message = ev.YeniDurum switch
            {
                "Yolda" => $"{ev.TrackingCode} takip kodlu kargonuz yola çıktı.",
                "Dağıtımda" => $"{ev.TrackingCode} takip kodlu kargonuz dağıtımda.",
                "Teslim Edildi" => $"{ev.TrackingCode} takip kodlu kargonuz teslim edildi.",
                _ => $"{ev.TrackingCode} takip kodlu kargonuzun durumu güncellendi: {ev.YeniDurum}"
            };

            if (await context.Notifications.AnyAsync(n =>
                    n.ShipmentId == ev.ShipmentId && n.Message == message))
            {
                _logger.LogInformation(
                    "Bildirim zaten mevcut, tekrar islenmedi: {TrackingCode}",
                    ev.TrackingCode);
                return;
            }

            var notification = new Notification
            {
                ShipmentId = ev.ShipmentId,
                BranchId = ev.BranchId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            // Mail gönder
            if (!string.IsNullOrEmpty(ev.ReceiverEmail))
            {
                await _emailService.SendShipmentStatusEmailAsync(
                    ev.ReceiverEmail,
                    ev.ReceiverName ?? "Değerli Müşterimiz",
                    ev.TrackingCode,
                    ev.YeniDurum,
                    ev.DeliveryCode
                );
            }

            _logger.LogInformation("Bildirim olusturuldu: {Message}", message);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
                await _channel.DisposeAsync();
            if (_connection != null)
                await _connection.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}