using Dapper;
using SportsMockService.Context;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Repository
{
    public class GameOddRepository : IGameOddRepository
    {
        private readonly DapperContext _context;

        public GameOddRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GameOdd>> GetGameOdds(int? gameId)
        {
            string query = "";
            var parameters = new DynamicParameters();
            if (gameId != null)
            {
                parameters.Add("@GameId", gameId, DbType.Int32, ParameterDirection.Input);
                query = "SELECT * FROM GameOdds WHERE GameId = @GameId";
            }
            else
            {
                query = "SELECT * FROM GameOdds";
            }

            using (var connection = _context.CreateConnection())
            {
                var gameOdds = await connection.QueryAsync<GameOdd>(query, parameters);
                return gameOdds.ToList();
            }
        }

        public async Task<bool> GameOddExists(GameOdd gameOdd)
        {
            try
            {
                if (gameOdd.Game == null || gameOdd.Book == null)
                {
                    // Since Game and Book Nav properties are non nullable,
                    // it's impossible for an odd in the database to be stored that matches the gameOdd param.
                    return false;
                }

                var parameters = new DynamicParameters();
                parameters.Add("@GameId", gameOdd.Game.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@BookId", gameOdd.Book.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@OddType", gameOdd.OddType, DbType.String, ParameterDirection.Input, gameOdd.OddType.Length);

                GameOdd existingGameOdd = null;

                var sql = @"SELECT * FROM GameOdds WHERE BookId = @BookId AND GameId = @GameId AND OddType like @OddType";

                using (var connection = _context.CreateConnection())
                {
                    existingGameOdd = await connection.QueryFirstOrDefaultAsync<GameOdd>(sql, parameters);
                    return existingGameOdd != null;
                }
            }
            catch(Exception ex)
            {
                return true;
            }
        }

        public async Task<int> UpdateGameOdd(GameOdd gameOdd, string changeCategory, string innerCategory)
        {
            /*
                 innerCategory: HomeMoneyLine, AwayMoneyLine, DrawMoneyLine, HomeSpread, AwaySpread, OverPayout, UnderPayout
            */
            try
            {
                if (String.IsNullOrEmpty(changeCategory))
                {
                    throw new Exception("changeCategory cannot be null. Valid values are moneyLine, spread, or overUnder");
                }
                bool exists = await GameOddExists(gameOdd);
                var parameters = new DynamicParameters();

                if (exists)
                {
                    if (gameOdd.Game == null || gameOdd.Book == null)
                    {
                        return -1; // Nav Property Cannot be null
                    }
                    // Get Linked Game
                    Game existingGame = null;
                    var sql = @"SELECT * FROM Games WHERE Id = @GameId";

                    parameters = new DynamicParameters();
                    parameters.Add("@GameId", gameOdd.Game.Id, DbType.Int32, ParameterDirection.Input);

                    using (var connection = _context.CreateConnection())
                    {
                        existingGame = await connection.QueryFirstOrDefaultAsync<Game>(sql, parameters);
                    }

                    // If Game Date is within an hour from now, don't allow change.
                    if ((existingGame.DateTime - DateTime.Now).TotalHours < 1.00)
                    {
                        // Do not allow update to be made to a game that starts within an hour (or has already started)
                        throw new Exception("Cannot update game that starts within 1 hour");
                    }

                    parameters = new DynamicParameters();
                    parameters.Add("@OddType", gameOdd.OddType, DbType.String, ParameterDirection.Input, gameOdd.OddType.Length);
                    parameters.Add("@Id", gameOdd.Id, DbType.Int32, ParameterDirection.Input);
                    Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();
                    int i = 0;
                    switch (changeCategory)
                    {
                        case "moneyLine":
                            string[] keys = String.IsNullOrEmpty(innerCategory) ? (new string[] { "@HomeMoneyLine", "@AwayMoneyLine", "@DrawMoneyLine" }) : 
                                (new string[] { String.Concat("@", innerCategory) });

                            if (String.IsNullOrEmpty(innerCategory) || innerCategory.Equals("HomeMoneyLine"))
                            {
                                dictionary.Add(keys[i], gameOdd.HomeMoneyLine);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                                i++;
                            }

                            if (String.IsNullOrEmpty(innerCategory) || innerCategory.Equals("AwayMoneyLine"))
                            {
                                dictionary.Add(keys[i], gameOdd.AwayMoneyLine);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                                i++;
                            }

                            if (String.IsNullOrEmpty(innerCategory) || innerCategory.Equals("DrawMoneyLine"))
                            {
                                dictionary.Add(keys[i], gameOdd.DrawMoneyLine);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                                i++;
                            }

                            break;
                        case "spread":
                            keys = String.IsNullOrEmpty(innerCategory) ? (new string[] { "@HomePointSpread", "@AwayPointSpread", "@HomePointSpreadPayout", "@AwayPointSpreadPayout" }) :
                                (innerCategory.Equals("HomeSpread") ? (new string[] { "@HomePointSpread", "@HomePointSpreadPayout" }) : (new string[] { "@AwayPointSpread", "@AwayPointSpreadPayout" }));

                            if (String.IsNullOrEmpty(innerCategory))
                            {
                                dictionary.Add(keys[i], gameOdd.HomePointSpread);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;

                                dictionary.Add(keys[i], gameOdd.HomePointSpreadPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;

                                dictionary.Add(keys[i], gameOdd.AwayPointSpread);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;

                                dictionary.Add(keys[i], gameOdd.AwayPointSpreadPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;
                            }
                            else if (innerCategory.Equals("HomeSpread"))
                            {
                                dictionary.Add(keys[i], gameOdd.HomePointSpread);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;

                                dictionary.Add(keys[i], gameOdd.HomePointSpreadPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;
                            }
                            else if (innerCategory.Equals("AwaySpread"))
                            {
                                dictionary.Add(keys[i], gameOdd.AwayPointSpread);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;

                                dictionary.Add(keys[i], gameOdd.AwayPointSpreadPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Decimal, ParameterDirection.Input);
                                i++;
                            }

                            break;
                        case "overUnder":
                            keys = String.IsNullOrEmpty(innerCategory) ? new string[] { "@OverUnder", "@OverPayout", "@UnderPayout" } :
                                (new string[] { "@OverUnder", String.Concat("@", innerCategory) });

                            dictionary.Add(keys[i], gameOdd.OverUnder);
                            parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                            i++;

                            // OverUnder should change for both "Cards" but their Payouts should change individually
                            if (String.IsNullOrEmpty(innerCategory) || innerCategory.Equals("OverPayout"))
                            {
                                dictionary.Add(keys[i], gameOdd.OverPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                                i++;
                            }

                            if (String.IsNullOrEmpty(innerCategory) || innerCategory.Equals("UnderPayout"))
                            {
                                dictionary.Add(keys[i], gameOdd.UnderPayout);
                                parameters.Add(keys[i], dictionary[keys[i]], DbType.Int32, ParameterDirection.Input);
                                i++;
                            }

                            break;
                    }

                    string str = "";

                    foreach(KeyValuePair<string, dynamic> entry in dictionary)
                    {
                        str = String.Concat(str, String.Concat(entry.Key.ToString().Remove(0, 1), String.Concat(" = ", String.Concat(Convert.ToString(entry.Key), ", "))));
                    }

                    if (str.Length >= 2)
                    {
                        str = String.Concat(str.Remove(str.Length - 2), " ");
                        sql = String.Concat(String.Concat(@"UPDATE GameOdds SET ", str), "WHERE Id = @Id");

                        using (var connection = _context.CreateConnection())
                        {
                            var affectedRows = connection.Execute(sql, parameters);
                            Console.WriteLine($"Affected Rows: {affectedRows}");
                        }
                    }

                    return 0;
                }
                else
                {
                    throw new Exception("Game Odd DOES NOT EXIST");
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> AddGameOdd(GameOdd gameOdd)
        {
            try
            {
                // It should now allow a game odd to be added if a gameodd with the same (1) Book, GameId, and OddType exists
                bool exists = await GameOddExists(gameOdd);

                // If exists, verify that it's not too late to add game
                if (!exists)
                {
                    if (gameOdd.Game == null || gameOdd.Book == null)
                    {
                        return -1; // Nav Property Cannot be null
                    }

                    // Get Linked Game
                    Game existingGame = null;
                    var sql = @"SELECT * FROM Games WHERE Id = @GameId";
                    var parameters = new DynamicParameters();
                    parameters.Add("@GameId", gameOdd.Game.Id, DbType.Int32, ParameterDirection.Input);

                    using (var connection = _context.CreateConnection())
                    {
                        existingGame = await connection.QueryFirstOrDefaultAsync<Game>(sql, parameters);
                    }

                    DateTime currentTime = DateTime.Now;
                    if ((existingGame.DateTime - currentTime).TotalHours < 1.00)
                    {
                        return -1; // Cannot Add Game odd to a game that starts in less than 1 hour from now (or has already started)
                    }

                    parameters.Add("@BookId", gameOdd.Book.Id, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@GameId", gameOdd.Game.Id, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@Created", currentTime, DbType.DateTime, ParameterDirection.Input);
                    parameters.Add("@Updated", currentTime, DbType.DateTime, ParameterDirection.Input);
                    parameters.Add("@HomeMoneyLine", gameOdd.HomeMoneyLine != -1 ? gameOdd.HomeMoneyLine : null, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@AwayMoneyLine", gameOdd.AwayMoneyLine != -1 ? gameOdd.AwayMoneyLine : null, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@DrawMoneyLine", gameOdd.DrawMoneyLine != -1 ? gameOdd.DrawMoneyLine : null, DbType.Int32, ParameterDirection.Input);
                    parameters.Add("@HomePointSpread", gameOdd.HomePointSpread != -1 ? gameOdd.HomePointSpread : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@AwayPointSpread", gameOdd.AwayPointSpread != -1 ? gameOdd.AwayPointSpread : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@HomePointSpreadPayout", gameOdd.HomePointSpreadPayout != -1 ? gameOdd.HomePointSpreadPayout : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@AwayPointSpreadPayout", gameOdd.AwayPointSpreadPayout != -1 ? gameOdd.AwayPointSpreadPayout : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@OverUnder", gameOdd.OverUnder != -1 ? gameOdd.OverUnder : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@OverPayout", gameOdd.OverPayout != -1 ? gameOdd.OverPayout : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@UnderPayout", gameOdd.UnderPayout != -1 ? gameOdd.UnderPayout : null, DbType.Decimal, ParameterDirection.Input);
                    parameters.Add("@OddType", gameOdd.OddType, DbType.String, ParameterDirection.Input, gameOdd.OddType.Length);


                    sql = @"INSERT INTO GameOdds([BookId], [GameId], [Created], [Updated], [HomeMoneyLine], [AwayMoneyLine], [DrawMoneyLine], 
                        [HomePointSpread], [AwayPointSpread], [HomePointSpreadPayout], [AwayPointSpreadPayout], [OverUnder], [OverPayout], [UnderPayout], [OddType])
                        VALUES(@BookId, @GameId, @Created, @Updated, @HomeMoneyLine, @AwayMoneyLine, @DrawMoneyLine, @HomePointSpread, @AwayPointSpread, 
                        @HomePointSpreadPayout, @AwayPointSpreadPayout, @OverUnder, @OverPayout, @UnderPayout, @OddType);";
                    using (var connection = _context.CreateConnection())
                    {
                        var affectedRows = connection.Execute(sql, parameters);
                        Console.WriteLine($"Affected Rows: {affectedRows}");
                    }

                }
                else
                {
                    return -1; // Cannot add a GameOdd whose Composite Key already exists
                }
                return 0;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }
    }
}
