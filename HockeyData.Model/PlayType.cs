using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public enum PlayType
	{
		Unknown = 99,
		GameScheduled = 1,
		PeriodReady = 2,
		PeriodStart = 3,
		PeriodEnd = 4,
		PeriodOfficial = 5,
		GameEnd = 6,
		Faceoff = 10,
		Stoppage = 11,
		Penalty = 12,
		Hit = 13,
		Giveaway = 14,
		Takeaway = 15,
		Shot = 20,
		Goal = 21,
		MissedShot = 22,
		BlockedShot = 23,
	}

	public class RefPlayType
	{
		public PlayType PlayTypeId { get; set; }
		public string PlayTypeName { get; set; }
		public string NhlCode { get; set; }
	}
}
