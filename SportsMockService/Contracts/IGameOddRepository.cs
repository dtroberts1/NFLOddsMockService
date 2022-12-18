using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Contracts
{
    public interface IGameOddRepository
    {
        public Task<IEnumerable<GameOdd>> GetGameOdds(int? gameId);
        public Task<int> AddGameOdd(GameOdd gameOdd);
        public Task<int> UpdateGameOdd(GameOdd gameOdd, string changeCategory, string innerCategory);
    }
}
