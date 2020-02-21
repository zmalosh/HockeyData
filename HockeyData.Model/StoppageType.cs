using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Model
{
	public enum StoppageType
	{
		Unknown = 99,
		GoalieStopped = 1,
		PuckFrozen = 2,
		Offside = 3,
		Icing = 4,
		PuckInBenches = 5,
		PuckInCrowd = 6,
		PuckInNetting = 7,
		NetOff = 8,
		TVTimeout = 9
	}

	public class RefStoppageType
	{
		public StoppageType StoppageTypeId { get; set; }
		public string StoppageName { get; set; }
		public string NhlDescription { get; set; }
	}
}
