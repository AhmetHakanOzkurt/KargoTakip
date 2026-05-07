using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
