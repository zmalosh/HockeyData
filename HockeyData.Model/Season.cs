using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class Season : IEntity
	{
		public int SeasonId { get; set; }
		public int LeagueId { get; set; }
		public bool HasTies { get; set; }
		public bool HasConferences { get; set; }
		public bool HasDivisions { get; set; }
		public bool HasWildCard { get; set; }
		public bool HasOlympics { get; set; }
		public string NhlSeasonKey { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual League League { get; set; }
		public virtual IList<Game> Games { get; set; }
	}
}
