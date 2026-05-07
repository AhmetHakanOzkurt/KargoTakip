namespace NotificationService.Messaging
{
    public class KargoOlusturulduEvent
    {
        public int ShipmentId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
    }

    public class KargoDurumuGuncellendiEvent
    {
        public int ShipmentId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string EskiDurum { get; set; } = string.Empty;
        public string YeniDurum { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public DateTime GuncellemeTarihi { get; set; }
    }
}