using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class Player : IEntity
	{
		public int PlayerId { get; set; }
		public string FullName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsActive { get; set; }
		public string PrimaryPosition { get; set; }
		public int? Height { get; set; }
		public int? Weight { get; set; }
		public string Nationality { get; set; }
		public string Handedness { get; set; }
		public DateTime BirthDate { get; set; }
		public string BirthCountry { get; set; }
		public string BirthState { get; set; }
		public string BirthCity { get; set; }
		public int? JerseyNumber { get; set; }
		public int NhlPlayerId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public IList<SkaterBoxscore> PlayerBoxscores { get; set; }
		public IList<GoalieBoxscore> GoalieBoxscores { get; set; }
		public IList<GamePlay> GamePlays { get; set; }
		public IList<GamePlay> OppGamePlays { get; set; }
		public IList<GamePlay> Assist1Plays { get; set; }
		public IList<GamePlay> Assist2Plays { get; set; }
	}
}
