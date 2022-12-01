using Dapper;
using SportsMockService.Context;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Repository
{
    public class VenueRepository : IVenueRepository
    {
        private readonly DapperContext _context;

        public VenueRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetVenues()
        {
            var query = "SELECT * FROM Venues";

            using (var connection = _context.CreateConnection())
            {
                var venues = await connection.QueryAsync<Venue>(query);
                return venues.ToList();
            }
        }
    }
}
