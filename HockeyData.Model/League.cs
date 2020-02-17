using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class League
	{
		public int LeagueId { get; set; }
		public string LeagueName { get; set; }
		public string LeagueAbbr { get; set; }

		public virtual IList<Season> Seasons { get; set; }
	}
}
