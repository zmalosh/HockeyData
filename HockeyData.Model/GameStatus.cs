using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public enum GameStatus
	{
		Pregame = 1,
		Live = 2,
		Final = 3,
		Postponed = 4
	}

	public enum DetailedGameStatus
	{
		Scheduled = 10,
		Pregame = 11,
		ScheduledTbd = 12,
		InProgress = 20,
		InProgressCritical = 21,
		GameOver = 30,
		Final = 31,
		Final2 = 32,
		Postponed = 40,
	}

	public class RefGameStatus
	{
		public DetailedGameStatus DetailedGameStatusId { get; set; }
		public GameStatus GameStatusId { get; set; }
		public int NhlStatusCode { get; set; }
		public string GameStatusName { get; set; }
		public string DetailedGameStatusName { get; set; }
	}
}
