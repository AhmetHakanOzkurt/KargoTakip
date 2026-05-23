using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class CityDistance
    {
        public int Id { get; set; }
        public int FromCityId { get; set; }
        public int ToCityId { get; set; }
        public int DistanceKm { get; set; }
        public decimal EstimatedHours { get; set; }

        public City FromCity { get; set; } = null!;
        public City ToCity { get; set; } = null!;
    }
}
