using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class TransferRequestItem
    {
        public int Id { get; set; }
        public int TransferRequestId { get; set; }
        public int ShipmentId { get; set; }

        public TransferRequest TransferRequest { get; set; } = null!;
        public Shipment Shipment { get; set; } = null!;
    }
}
