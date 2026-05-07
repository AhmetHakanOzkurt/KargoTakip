using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int? ShipmentId { get; set; }
        public int? TransferRequestId { get; set; }
        public int BranchId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Shipment? Shipment { get; set; }
        public TransferRequest? TransferRequest { get; set; }
        public Branch Branch { get; set; } = null!;
    }
}
