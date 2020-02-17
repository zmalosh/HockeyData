using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class GoalieBoxscore : IEntity
	{
		public int GameId { get; set; }
		public int PlayerId { get; set; }
		public int TeamId { get; set; }
		public int? JerseyNumber { get; set; }
		public string Decision { get; set; }
		public int? Saves { get; set; }
		public int? Shots { get; set; }
		public int? GoalsAllowed { get; set; }
		public int? PenaltyMinutes { get; set; }
		public int? TimeOnIce { get; set; }
		public int? SavesEV { get; set; }
		public int? SavesPP { get; set; }
		public int? SavesSH { get; set; }
		public int? ShotsEV { get; set; }
		public int? ShotsPP { get; set; }
		public int? ShotsSH { get; set; }
		public int? GoalsAllowedEV { get; set; }
		public int? GoalsAllowedPP { get; set; }
		public int? GoalsAllowedSH { get; set; }
		public int? GoalsScored { get; set; }
		public int? AssistsScored { get; set; }
		public int? StarNumber { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual Player Player { get; set; }
		public virtual Game Game { get; set; }
		public virtual Team Team { get; set; }

	}
}
