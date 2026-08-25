using System.Text.Json;
using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;

namespace OrderService.Messaging
{
    /// <summary>
    /// Event'i is verisiyle ayni DbContext uzerinden yazar. SaveChangesAsync
    /// cagrisi ikisini tek transaction'da commit ettigi icin "kayit yazildi
    /// ama event kayboldu" durumu ortadan kalkar.
    ///
    /// ONEMLI: Ekleme yapildiktan sonra SaveChangesAsync'i cagiran taraf
    /// sorumludur; bu sinif kendi basina kaydetmez.
    /// </summary>
    public static class OutboxYazici
    {
        public static void EventEkle(
            this KargoTakipDbContext context, string queueName, object payload)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                QueueName = queueName,
                Payload = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
