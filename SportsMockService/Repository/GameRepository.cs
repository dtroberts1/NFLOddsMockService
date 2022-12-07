using Dapper;
using SportsMockService.Context;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SportsMockService.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly DapperContext _context;
        private readonly IConfiguration _configuration;

        public GameRepository(DapperContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> GameExists(int? gameId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GameId", gameId, DbType.Int32, ParameterDirection.Input);
            Game game = null;

            var sql = @"SELECT g.Id
              FROM Games g WHERE Id = @GameId";
            using (var connection = _context.CreateConnection())
            {
                game = await connection.QueryFirstOrDefaultAsync<Game>(sql, parameters);
                return game != null;
            }
        }

        public int GetAPIWeek(DateTime? existingDate)
        {
            DateTime gameDate = (DateTime)(existingDate != null ? existingDate : DateTime.Now);
            DateTime seedDate = DateTime.Parse(this._configuration.GetSection("SportsMock").GetSection("SeedDate")?.Value ?? "");
            return (gameDate - seedDate).Days + 1;
        }

        public async Task<int> PutGame(Game game)
        {
            try
            {
                bool exists = await GameExists(game.Id);
                if (exists)
                {
                    // If Game Date is within an hour from now, don't allow change.
                    if ((game.DateTime - DateTime.Now ).TotalHours < 1.00)
                    {
                        // Do not allow update to be made to a game that starts within an hour (or has already started)
                        throw new Exception("Cannot update game that starts within 1 hour");
                    }
                    int apiWeek = GetAPIWeek(game.DateTime);
                    var parameters = new DynamicParameters();

                    parameters.Add("@Id", game.Id, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@APIWeek", apiWeek, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@DateTime", game.DateTime, DbType.DateTime, ParameterDirection.Input);
                    parameters.Add("@HomeTeamId", game.HomeTeam.Id, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@AwayTeamId", game.AwayTeam.Id, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@StatusId", game.Status.Id, DbType.Int32, ParameterDirection.Input);

                    string sql = @"UPDATE Games SET APIWeek = @APIWeek, DateTime = @DateTime, 
                        HomeTeamId = @HomeTeamId, AwayTeamId = @AwayTeamId, StatusId = @StatusId WHERE Id = @Id";
                    using (var connection = _context.CreateConnection())
                    {
                        var affectedRows = connection.Execute(sql, parameters);
                        Console.WriteLine($"Affected Rows: {affectedRows}");
                    }
                    return 0;
                }
                else
                {
                    throw new Exception("Game does not exist");
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> AddGame(Game game)
        {
            bool exists = await GameExists(game.Id);

            if (!exists)
            {
                int apiWeek = GetAPIWeek(null);
                var parameters = new DynamicParameters();
                int? statusId = -1;

                var sql = @"SELECT Id
                    FROM Statuses s WHERE s.[StatusText] like 'SCHEDULED'";
                using (var connection = _context.CreateConnection())
                {
                    statusId = await connection.QueryFirstOrDefaultAsync<int>(sql);
                }

                if (statusId == null)
                {
                    return -1;
                }

                parameters.Add("@APIWeek", apiWeek, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@DateTime", DateTime.Now, DbType.DateTime, ParameterDirection.Input);
                parameters.Add("@HomeTeamId", game.HomeTeam.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@AwayTeamId", game.AwayTeam.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@StatusId", statusId, DbType.Int32, ParameterDirection.Input);

                sql = @"  INSERT INTO Games(StatusId, SeasonType, Season, APIWeek, [DateTime], HomeTeamId, AwayTeamId)
                    VALUES(@StatusId, 1, '2022', @APIWeek, @DateTime, @HomeTeamId, @AwayTeamId); ";
                using (var connection = _context.CreateConnection())
                {
                    var affectedRows = connection.Execute(sql, parameters);
                    Console.WriteLine($"Affected Rows: {affectedRows}");
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public async Task<IEnumerable<Game>> GetGames()
        {
            IEnumerable<Game> games = null;
            IEnumerable<Game> duplicateGames = null;

            var sql = @"SELECT g.Id, s.Id, s.StatusText,
                hT.Id, hT.Name, aT.Id, aT.Name, 
                hVenue.Id, hVenue.Name, hVenue.Capacity, hVenue.City, hVenue.MapCoordinates, hVenue.CountryCode,
                aVenue.Id, aVenue.Name, aVenue.Capacity, aVenue.City, aVenue.MapCoordinates, aVenue.CountryCode,
                gO.Id, gO.[Created]
                  ,gO.[Updated]
                  ,gO.[HomeMoneyLine]
                  ,gO.[DrawMoneyLine]
                  ,gO.[HomePointSpread]
                  ,gO.[AwayPointSpread]
                  ,gO.[HomePointSpreadPayout]
                  ,gO.[AwayPointSpreadPayout]
                  ,gO.[OverUnder]
                  ,gO.[OverPayout]
                  ,gO.[UnderPayout]
                  ,gO.[AwayMoneyLine]
              FROM Games g
              INNER JOIN Statuses s
              ON s.Id = g.StatusId
			  INNER JOIN Teams hT
			  ON hT.Id = g.HomeTeamId
              INNER JOIN Teams aT
              ON aT.Id = g.AwayTeamId
              INNER JOIN Venues hVenue
              ON hVenue.Id = hT.VenueId
              INNER JOIN Venues aVenue
              ON aVenue.Id = aT.VenueId
              LEFT OUTER JOIN GameOdds gO
              ON gO.GameId = g.Id";
            using (var connection = _context.CreateConnection())
            {
                duplicateGames = await connection.QueryAsync<Game, Status, Team, Team, Venue, Venue, GameOdd, Game>(sql, (game, status, homeTeam, awayTeam, homeVenue, awayVenue, gameOdd) => {
                    homeTeam.Venue = homeVenue;
                    awayTeam.Venue = awayVenue;
                    game.HomeTeam = homeTeam;
                    game.AwayTeam = awayTeam;
                    game.Status = status;
                    if (gameOdd != null)
                    {
                        game.AllGameOdds.Add(gameOdd);
                    }
                    return game;
                },
            splitOn: "Id, Id, Id, Id, Id, Id");
                games = duplicateGames.GroupBy(g => g.Id).Select(group =>
                {
                    var groupedGame = group.First();
                    groupedGame.AllGameOdds = group.Where(currGame => currGame.AllGameOdds.Count > 0).Select(currGame => currGame.AllGameOdds.Single()).ToList();

                    return groupedGame;
                });
            }

            return games;
        }
        /*
        public async Task<IEnumerable<Game>> GetGamesWithCompetitors()
        {
            IEnumerable<Game> games = null;
            IEnumerable<Game> duplicateGames = null;

            var sql = @"SELECT g.EventDate, g.StartTimeTBD, g.Id as 'GameId', s.Id as 'StatusId', s.StatusText,
                c.Id AS 'CompetitorId', c.Qualifier,
                t.Id AS 'TeamId', t.Name, v.Id AS 'VenueId', v.Name, v.Capacity, v.City, v.MapCoordinates, v.CountryCode
              FROM Games g
              INNER JOIN Statuses s
              ON s.Id = g.StatusId
              INNER JOIN Competitors c
			  ON c.GameId = g.Id
			  INNER JOIN Teams t
			  ON t.Id = c.TeamId
              INNER JOIN Venues v
              ON v.Id = t.VenueId";
            using (var connection = _context.CreateConnection())
            {
                duplicateGames = await connection.QueryAsync<Game, Status, Competitor, Team, Venue, Game>(sql, (game, status, competitor, team, venue) => {
                    game.Status = status;
                    team.Venue = venue;
                    competitor.Team = team;
                    game.Competitors.Add(competitor);
                    return game;
                },
            splitOn: "StatusId, CompetitorId, TeamId, VenueId");
                games = duplicateGames.GroupBy(g => g.Id).Select(group =>
                {
                    var groupedGame = group.First();
                    groupedGame.Competitors = group.Select(currGame => currGame.Competitors.Single()).ToList();
                    return groupedGame;
                });
            }

            return games;
        }
        */
    }
}
