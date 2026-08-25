using KargoTakip.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Messaging
{
    /// <summary>
    /// Outbox tablosundaki islenmemis event'leri sirayla RabbitMQ'ya aktarir.
    /// Publish basarisiz olursa kayit islenmemis kalir ve bir sonraki turda
    /// yeniden denenir; boylece event kaybolmaz.
    /// </summary>
    public class OutboxYayinlayici : BackgroundService
    {
        private static readonly TimeSpan Aralik = TimeSpan.FromSeconds(5);
        private const int PartiBoyutu = 50;
        private const int MaksDeneme = 10;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqProducer _producer;
        private readonly ILogger<OutboxYayinlayici> _logger;

        public OutboxYayinlayici(
            IServiceScopeFactory scopeFactory,
            RabbitMqProducer producer,
            ILogger<OutboxYayinlayici> logger)
        {
            _scopeFactory = scopeFactory;
            _producer = producer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox yayinlayici basladi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PartiyiIsleAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Tek bir tur basarisiz olsa da dongu devam etmeli.
                    _logger.LogError(ex, "Outbox turu basarisiz oldu.");
                }

                await Task.Delay(Aralik, stoppingToken);
            }
        }

        private async Task PartiyiIsleAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<KargoTakipDbContext>();

            var bekleyenler = await context.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.Attempts < MaksDeneme)
                .OrderBy(m => m.Id)
                .Take(PartiBoyutu)
                .ToListAsync(stoppingToken);

            if (bekleyenler.Count == 0)
                return;

            foreach (var mesaj in bekleyenler)
            {
                try
                {
                    await _producer.PublishRawAsync(mesaj.QueueName, mesaj.Payload);

                    mesaj.ProcessedAt = DateTime.UtcNow;
                    mesaj.LastError = null;
                }
                catch (Exception ex)
                {
                    mesaj.Attempts++;
                    mesaj.LastError = ex.Message.Length > 1000
                        ? ex.Message[..1000]
                        : ex.Message;

                    _logger.LogWarning(ex,
                        "Outbox mesaji yayinlanamadi (deneme {Attempts}/{Max}): {Id} -> {Queue}",
                        mesaj.Attempts, MaksDeneme, mesaj.Id, mesaj.QueueName);

                    if (mesaj.Attempts >= MaksDeneme)
                        _logger.LogError(
                            "Outbox mesaji {Id} maksimum deneme sayisina ulasti, " +
                            "artik denenmeyecek. Kuyruk: {Queue}",
                            mesaj.Id, mesaj.QueueName);

                    // Baglanti sorunu ise sonraki mesajlar da basarisiz olur;
                    // turu bitirip bir sonraki dongude yeniden dene.
                    await context.SaveChangesAsync(stoppingToken);
                    return;
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
