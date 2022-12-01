using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Entities
{
    public class Game
    {
        public int Id { get; set; } /* GlobalGameId and ScoreId */
        public DateTime DateTime { get; set; }
        public Status Status { get; set; }
        public int Season { get; set; }
        public int SeasonType { get; set; }
        public int APIWeek { get; set; }

        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }

        /*
        		"AwayTeamName": "BUF",
		"HomeTeamName": "DET",
		"GlobalGameId": 18088,
		"GlobalAwayTeamId": 4,
		"GlobalHomeTeamId": 11,
		"HomeTeamScore": 19,
		"AwayTeamScore": 22,
        		"HomeRotationNumber": 82,
		"AwayRotationNumber": 82,
                */
        public List<GameOdd> AllGameOdds { get; set; } = new List<GameOdd>();


    }
}
