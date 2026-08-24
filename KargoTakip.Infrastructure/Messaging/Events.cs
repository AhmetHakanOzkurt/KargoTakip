namespace KargoTakip.Infrastructure.Messaging
{
    // Bu sozlesmeler OrderService ve NotificationService'te birebir kopyaydi;
    // birinde degisiklik yapilip digeri unutuldugunda mesajlar sessizce
    // deserialize edilemiyordu. Tek kaynak burasidir.

    public class KargoOlusturulduEvent
    {
        public int ShipmentId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
        public string? ReceiverEmail { get; set; }
    }

    public class KargoDurumuGuncellendiEvent
    {
        public int ShipmentId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string EskiDurum { get; set; } = string.Empty;
        public string YeniDurum { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public DateTime GuncellemeTarihi { get; set; }
        public string? ReceiverEmail { get; set; }
        public string? ReceiverName { get; set; }
        public string? DeliveryCode { get; set; }
    }
}
