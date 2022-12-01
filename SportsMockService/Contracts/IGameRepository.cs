using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SportsMockService.Contracts
{
    public interface IGameRepository
    {

        public Task<IEnumerable<Game>> GetGames();
        public Task<bool> GameExists(int? gameId);
        public Task<int> AddGame(Game game);
        public Task<int> PutGame(Game game);
    }
}
