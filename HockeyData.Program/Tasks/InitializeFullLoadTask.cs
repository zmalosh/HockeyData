using HockeyData.Model;
using HockeyData.Processors.NhlCom.Processors;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Program.Tasks
{
	public class InitializeFullLoadTask : BaseTask
	{
		public override void Run()
		{
			Console.WriteLine("Hello World!");

			var config = GetConfig();

			using (var context = new HockeyDataContext(config))
			{
				context.Database.EnsureDeleted();
				context.Database.EnsureCreated();
				context.SaveChanges();

				context.Leagues.Add(new League { LeagueName = "National Hockey League", LeagueAbbr = "NHL" });
				context.SaveChanges();

				var seasonsProcessor = new SeasonsProcessor();
				seasonsProcessor.Run(context);
			}
		}
	}
}
