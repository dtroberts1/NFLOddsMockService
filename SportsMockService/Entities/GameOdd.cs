using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Entities
{
    public class GameOdd
    {
        public int Id { get; set; } /* GameOddId */
        public string OddType { get; set; }
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }

		public int? HomeMoneyLine { get; set; }
        public int? AwayMoneyLine { get; set; }
		public int? DrawMoneyLine { get; set; }

		public decimal? HomePointSpread { get; set; }
		public decimal? AwayPointSpread { get; set; }
		public decimal? HomePointSpreadPayout { get; set; }
		public decimal? AwayPointSpreadPayout { get; set; }
		public decimal? OverUnder { get; set; }
		public int? OverPayout { get; set; }
		public int? UnderPayout { get; set; }

        public Book Book { get; set; }
		public Game Game { get; set; }
    }
}
