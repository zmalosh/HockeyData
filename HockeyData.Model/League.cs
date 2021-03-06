﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public class League : IEntity
	{
		public int LeagueId { get; set; }
		public string LeagueName { get; set; }
		public string LeagueAbbr { get; set; }
		public int? NhlLeagueId { get; set; }
		public DateTime DateLastModifiedUtc { get; set; }
		public DateTime DateCreatedUtc { get; set; }

		public virtual IList<Season> Seasons { get; set; }
	}
}
