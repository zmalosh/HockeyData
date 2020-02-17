using HockeyData.Model;
using HockeyData.Processors.NhlCom.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
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

				context.RefGameTypes.AddRange(new List<RefGameType>
				{
					new RefGameType{ GameTypeId = GameType.Unknown, NhlGameTypeKey = string.Empty, GameTypeDescription = "Unknown"},
					new RefGameType{ GameTypeId = GameType.RegularSeason, NhlGameTypeKey = "R", GameTypeDescription = "Regular Season"},
					new RefGameType{ GameTypeId = GameType.Postseason, NhlGameTypeKey = "P", GameTypeDescription = "Postseason"},
					new RefGameType{ GameTypeId = GameType.Preseason, NhlGameTypeKey = "PR", GameTypeDescription = "Preseason"},
					new RefGameType{ GameTypeId = GameType.AllStarGame, NhlGameTypeKey = "A", GameTypeDescription = "All-Star Game"},
					new RefGameType{ GameTypeId = GameType.AllStarGameWomen, NhlGameTypeKey = "WA", GameTypeDescription = "All-Star Women Game"},
					new RefGameType{ GameTypeId = GameType.Olympics, NhlGameTypeKey = "O", GameTypeDescription = "Olympics Game"},
					new RefGameType{ GameTypeId = GameType.Exhibition, NhlGameTypeKey = "E", GameTypeDescription = "Exhibition"},
					new RefGameType{ GameTypeId = GameType.WorldCupExhibition, NhlGameTypeKey = "WCOH_EXH", GameTypeDescription = "World Cup of Hockey exhibition/preseason games "},
					new RefGameType{ GameTypeId = GameType.WorldCupPrelim, NhlGameTypeKey = "WCOH_PRELIM", GameTypeDescription = "World Cup of Hockey preliminary games"},
					new RefGameType{ GameTypeId = GameType.WorldCupFinal, NhlGameTypeKey = "WCOH_FINAL", GameTypeDescription = "World Cup of Hockey semifinals and finals"},
				});
				context.SaveChanges();

				context.Leagues.Add(new League { LeagueName = "National Hockey League", LeagueAbbr = "NHL" });
				context.SaveChanges();

				var seasonsProcessor = new SeasonsProcessor();
				seasonsProcessor.Run(context);

				var dbSeasons = context.Seasons.OrderBy(x => x.SeasonId).ToList();
				foreach (var dbSeason in dbSeasons)
				{
					var nhlSeasonKey = dbSeason.NhlSeasonKey;
					Console.WriteLine($"PROCESS TEAMS - {nhlSeasonKey}");
					var teamsProcessor = new TeamsProcessor(nhlSeasonKey);
					teamsProcessor.Run(context);
				}
			}
		}
	}
}
