using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class ConsolidationPlan
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int OriginBranchId { get; set; }
        public int DestinationCityId { get; set; }
        public string Status { get; set; } = "Planlandı";
        public DateTime PlannedDepartureAt { get; set; }
        public DateTime? ActualDepartureAt { get; set; }
        public int TotalCapacity { get; set; }
        public int UsedCapacity { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal EstimatedFuelSaving { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Vehicle Vehicle { get; set; } = null!;
        public Branch OriginBranch { get; set; } = null!;
        public City DestinationCity { get; set; } = null!;
        public ICollection<ConsolidationPlanItem> Items { get; set; } = new List<ConsolidationPlanItem>();
    }
}
