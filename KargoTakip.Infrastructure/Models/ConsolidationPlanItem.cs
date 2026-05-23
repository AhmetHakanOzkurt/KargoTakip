using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class ConsolidationPlanItem
    {
        public int Id { get; set; }
        public int ConsolidationPlanId { get; set; }
        public int ShipmentId { get; set; }
        public string AddedReason { get; set; } = "PrimaryDestination";

        public ConsolidationPlan ConsolidationPlan { get; set; } = null!;
        public Shipment Shipment { get; set; } = null!;
    }
}
