using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentLoad { get; set; } = 0;
        public int VehicleTypeId { get; set; }
        public int BranchId { get; set; }
        public int CityId { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public VehicleType VehicleType { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public City City { get; set; } = null!;
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    }
}
