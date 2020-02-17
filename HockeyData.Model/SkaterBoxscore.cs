using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class SkaterBoxscore : IEntity
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int TeamId { get; set; }
		public string Position { get; set; }
		public int? JerseyNumber { get; set; }
		public int? Goals { get; set; }
		public int? Assists { get; set; }
		public int? Shots { get; set; }
		public int? PlusMinus { get; set; }
		public int? OppShotsBlocked { get; set; }
		public int? Hits { get; set; }
		public int? PenaltyMinutes { get; set; }
		public int? Giveaways { get; set; }
		public int? Takeaways { get; set; }
		public int? FaceoffsTaken { get; set; }
		public int? FaceoffsWon { get; set; }
		public int? GoalsEV { get; set; }
		public int? GoalsPP { get; set; }
		public int? GoalsSH { get; set; }
		public int? AssistsEV { get; set; }
		public int? AssistsPP { get; set; }
		public int? AssistsSH { get; set; }
		public int? IceTimeTotal { get; set; }
		public int? IceTimeEV { get; set; }
		public int? IceTimePP { get; set; }
		public int? IceTimeSH { get; set; }
		public int? StarNumber { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Player Player { get; set; }
		public virtual Game Game { get; set; }
		public virtual Team Team { get; set; }
	}
}
