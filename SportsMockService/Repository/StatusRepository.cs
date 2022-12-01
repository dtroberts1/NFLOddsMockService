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
    public class StatusRepository : IStatusRepository
    {
        private readonly DapperContext _context;

        public StatusRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Status>> GetStatuses()
        {
            var query = "SELECT * FROM Statuses";

            using (var connection = _context.CreateConnection())
            {
                var statuses = await connection.QueryAsync<Status>(query);
                return statuses.ToList();
            }
        }
    }
}
