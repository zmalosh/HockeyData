using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class GamePlay : IEntity
	{
		public int GamePlayId { get; set; }
		public int GameId { get; set; }
		public int PlaySeq { get; set; } // eventIdx
		public PlayType PlayTypeId { get; set; }
		public int PeriodNum { get; set; }
		public int PeriodTimeRem { get; set; }
		public int PeriodTime { get; set; }
		public int NhlEventId { get; set; }
		public string NhlEventCode { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public string PlayDesc { get; set; }
		public int? HomeScore { get; set; }
		public int? AwayScore { get; set; }
		public decimal? XCoord { get; set; }
		public decimal? YCoord { get; set; }
		public int? TeamId { get; set; }
		public int? PlayerId { get; set; }
		public int? OppPlayerId { get; set; }
		public string PenaltyName { get; set; }
		public string PenaltySeverity { get; set; }
		public int? PenaltyMins { get; set; }
		public string ShotType { get; set; }
		public bool? IsGoal { get; set; }
		public bool? IsShootout { get; set; }
		public bool? IsEmptyNet { get; set; }
		public int? Assist1PlayerId { get; set; }
		public int? Assist2PlayerId { get; set; }
		public string StrengthType { get; set; }
		public DateTime? WallTimeUtc { get; set; }
		public int? OppTeamId { get; set; }
		public decimal? NhlXCoord { get; set; }
		public decimal? NhlYCoord { get; set; }

		public virtual Game Game { get; set; }
		public virtual Team Team { get; set; }
		public virtual Team OppTeam { get; set; }
		public virtual Player Player { get; set; }
		public virtual Player OppPlayer { get; set; }
		public virtual Player Assist1Player { get; set; }
		public virtual Player Assist2Player { get; set; }
	}
}
