using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public int ReceiverCityId { get; set; }
        public decimal Weight { get; set; }
        public string Priority { get; set; } = "Normal";
        public string CurrentStatus { get; set; } = "Hazırlanıyor";
        public int BranchId { get; set; }
        public int? AssignedVehicleId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public City ReceiverCity { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public Vehicle? AssignedVehicle { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public ICollection<ShipmentStatusHistory> StatusHistories { get; set; } = new List<ShipmentStatusHistory>();
        public ICollection<TransferRequestItem> TransferRequestItems { get; set; } = new List<TransferRequestItem>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
