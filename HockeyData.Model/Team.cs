using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class Team : IEntity
	{
		public int TeamId { get; set; }
		public string TeamFullName { get; set; }
		public string TeamLocation { get; set; }
		public string TeamName { get; set; }
		public string TeamAbbr { get; set; }
		public string TeamShortName { get; set; }
		public int NhlTeamId { get; set; }
		public int? FirstYearOfPlay { get; set; }
		public string WebSiteUrl { get; set; }
		public string TimeZoneAbbr { get; set; }
		public string TimeZoneName { get; set; }
		public int? TimeZoneOffset { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual IList<Game> HomeGames { get; set; }
		public virtual IList<Game> AwayGames { get; set; }
		public IList<SkaterBoxscore> PlayerBoxscores { get; set; }
		public IList<GoalieBoxscore> GoalieBoxscores { get; set; }
		public IList<TeamBoxscore> TeamBoxscores { get; set; }
		public IList<TeamBoxscore> OppTeamBoxscores { get; set; }
		public IList<GamePlay> GamePlays { get; set; }
		public IList<GamePlay> OppGamePlays { get; set; }
	}
}
