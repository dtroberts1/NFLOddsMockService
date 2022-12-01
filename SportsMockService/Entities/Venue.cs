using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Entities
{
    public class Venue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string City { get; set; }
        public string MapCoordinates { get; set; }
        public string CountryCode { get; set; }

    }
}
