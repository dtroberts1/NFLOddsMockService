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
    public class TeamRepository : ITeamRepository
    {
        private readonly DapperContext _context;

        public TeamRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetTeams()
        {
            IEnumerable<Team> teams = null;
            var sql = @"  SELECT t.Name, t.Id, v.Id, v.Name, v.Capacity, v.City, v.MapCoordinates, v.CountryCode FROM Teams t
                INNER JOIN Venues v
                ON v.Id = t.VenueId;";
            using (var connection = _context.CreateConnection())
            {
                teams = await connection.QueryAsync<Team, Venue, Team>(sql, (team, venue) => {
                    team.Venue = venue;
                    return team;
                },
                splitOn: "Id");
            }

            return teams;
        }
    }
}
