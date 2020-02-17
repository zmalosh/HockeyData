using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public enum GameType
	{
		Unknown = 0,
		RegularSeason = 1,
		Postseason = 2,
		Preseason = 3,
		AllStarGame = 4,
		AllStarGameWomen = 5,
		Olympics = 6,
		Exhibition = 7,
		WorldCupExhibition = 8,
		WorldCupPrelim = 9,
		WorldCupFinal = 10
	}

	public class RefGameType
	{
		public GameType GameTypeId { get; set; }
		public string NhlGameTypeKey { get; set; }
		public string GameTypeDescription { get; set; }
	}
}
