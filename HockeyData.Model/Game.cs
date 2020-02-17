using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class Game
	{
		public int GameId { get; set; }
		public int SeasonId { get; set; }
		public int? HomeTeamId { get; set; }
		public int? AwayTeamId { get; set; }
		public GameType GameTypeId { get; set; }
		public DateTime GameDateEst { get; set; }
		public GameStatus GameStatusId { get; set; }
		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }
		public int? NhlVenueId { get; set; }
		public int NhlGameId { get; set; }
		public int? HomeGamesPlayed { get; set; }
		public int? HomeWins { get; set; }
		public int? HomeLosses { get; set; }
		public int? HomeTies { get; set; }
		public int? HomeOvertimes { get; set; }
		public int? AwayGamesPlayed { get; set; }
		public int? AwayWins { get; set; }
		public int? AwayLosses { get; set; }
		public int? AwayTies { get; set; }
		public int? AwayOvertimes { get; set; }
		public DateTime GameTimeUtc { get; set; }
		public DetailedGameStatus DetailedGameStatusId { get; set; }
		public string VenueName { get; set; }

		public virtual Season Season { get; set; }
		public virtual Team HomeTeam { get; set; }
		public virtual Team AwayTeam { get; set; }
	}
}
