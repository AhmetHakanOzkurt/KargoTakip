using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class ShipmentStatusHistory
    {
        public int Id { get; set; }
        public int ShipmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string ServiceSource { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public int ChangedByUserId { get; set; }

        public Shipment Shipment { get; set; } = null!;
        public User ChangedByUser { get; set; } = null!;
    }
}
