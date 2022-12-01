using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Contracts
{
    public interface IVenueRepository
    {
        public Task<IEnumerable<Venue>> GetVenues();
    }
}
