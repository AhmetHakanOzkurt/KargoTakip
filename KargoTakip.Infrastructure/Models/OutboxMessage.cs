namespace KargoTakip.Infrastructure.Models
{
    /// <summary>
    /// Event'ler DB commit'inden SONRA ayri bir adimda RabbitMQ'ya
    /// yayinlaniyordu. Publish basarisiz olursa veya servis o anda
    /// duserse event tamamen kayboluyordu; kargo olusuyor ama bildirim
    /// hic gitmiyordu.
    ///
    /// Artik event, is verisiyle AYNI transaction icinde bu tabloya
    /// yazilir; ayri bir arka plan servisi kuyruga aktarir. Teslimat
    /// "en az bir kez" garantisine doner, mukerrer teslim ihtimalini
    /// tuketici tarafindaki idempotency kontrolu karsilar.
    /// </summary>
    public class OutboxMessage
    {
        public long Id { get; set; }

        /// <summary>Hedef kuyruk adi.</summary>
        public string QueueName { get; set; } = string.Empty;

        /// <summary>JSON serilestirilmis event govdesi.</summary>
        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Dolu ise kuyruga basariyla aktarilmistir.</summary>
        public DateTime? ProcessedAt { get; set; }

        public int Attempts { get; set; }

        /// <summary>Son denemedeki hata; teshis icin saklanir.</summary>
        public string? LastError { get; set; }
    }
}
