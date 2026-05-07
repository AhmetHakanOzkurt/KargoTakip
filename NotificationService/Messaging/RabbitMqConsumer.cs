using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
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
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = hostName };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Kuyrukları tanımla
            await _channel.QueueDeclareAsync(
                queue: "kargo_olusturuldu",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            await _channel.QueueDeclareAsync(
                queue: "kargo_durumu_guncellendi",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

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

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
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

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
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

            var notification = new Notification
            {
                ShipmentId = ev.ShipmentId,
                BranchId = ev.BranchId,
                Message = $"{ev.TrackingCode} takip kodlu kargunuz hazırlanıyor.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            Console.WriteLine($"Bildirim oluşturuldu: {notification.Message}");
        }

        private async Task HandleKargoDurumuGuncellendi(KargoDurumuGuncellendiEvent ev)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<KargoTakipDbContext>();

            var message = ev.YeniDurum switch
            {
                "Yolda" => $"{ev.TrackingCode} takip kodlu kargunuz yola çıktı.",
                "Dağıtımda" => $"{ev.TrackingCode} takip kodlu kargunuz dağıtımda.",
                "Teslim Edildi" => $"{ev.TrackingCode} takip kodlu kargunuz teslim edildi.",
                _ => $"{ev.TrackingCode} takip kodlu kargunuzun durumu güncellendi: {ev.YeniDurum}"
            };

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

            Console.WriteLine($"Bildirim oluşturuldu: {notification.Message}");
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