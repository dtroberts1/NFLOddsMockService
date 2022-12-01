using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbrev { get; set; }
        public Venue Venue { get; set; }
        public List<Game> Games { get; set; }

    }
}
