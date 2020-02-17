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
			HockeyDataContext context = null;

			try
			{
				var config = GetConfig();

				context = new HockeyDataContext(config);

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

				context.RefGameStatuses.AddRange(new List<RefGameStatus>
				{
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.Scheduled, GameStatusId = GameStatus.Pregame, NhlStatusCode = 01, GameStatusName = "Pregame", DetailedGameStatusName = "Scheduled"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.Pregame, GameStatusId = GameStatus.Pregame, NhlStatusCode = 02, GameStatusName = "Pregame", DetailedGameStatusName = "Pregame"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.InProgress, GameStatusId = GameStatus.Live, NhlStatusCode = 03, GameStatusName = "Live", DetailedGameStatusName = "In-Progress"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.InProgressCritical, GameStatusId = GameStatus.Live, NhlStatusCode = 04, GameStatusName = "Live", DetailedGameStatusName = "In-Progress - Critical"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.GameOver, GameStatusId = GameStatus.Final, NhlStatusCode = 05, GameStatusName = "Final", DetailedGameStatusName = "Game Over"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.Final, GameStatusId = GameStatus.Final, NhlStatusCode = 06, GameStatusName = "Final", DetailedGameStatusName = "Final"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.Final2, GameStatusId = GameStatus.Final, NhlStatusCode = 07, GameStatusName = "Final", DetailedGameStatusName = "Final"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.ScheduledTbd, GameStatusId = GameStatus.Pregame, NhlStatusCode = 08, GameStatusName = "Pregame", DetailedGameStatusName = "Scheduled (Time TBD)"},
					new RefGameStatus{ DetailedGameStatusId = DetailedGameStatus.Postponed, GameStatusId = GameStatus.Postponed, NhlStatusCode = 09, GameStatusName = "Postponed", DetailedGameStatusName = "Postponed"},
				});
				context.SaveChanges();

				context.Leagues.Add(new League { LeagueName = "National Hockey League", LeagueAbbr = "NHL" });
				context.SaveChanges();

				var seasonsProcessor = new SeasonsProcessor();
				seasonsProcessor.Run(context);

				var dbSeasons = context.Seasons.OrderBy(x => x.SeasonId).ToList();
				for (int i = 0; i < dbSeasons.Count; i++)
				{
					var dbSeason = dbSeasons[i];
					var nhlSeasonKey = dbSeason.NhlSeasonKey;
					Console.WriteLine($"PROCESS TEAMS - {nhlSeasonKey}");
					var teamsProcessor = new TeamsProcessor(nhlSeasonKey);
					teamsProcessor.Run(context);

					Console.WriteLine($"PROCESS GAMES - {nhlSeasonKey}");
					var gamesProcessor = new GamesProcessor(nhlSeasonKey);
					gamesProcessor.Run(context);

					if (i % 10 == 9)
					{
						context.Dispose();
						context = new HockeyDataContext(config);
					}
				}
			}
			finally
			{
				if (context != null)
				{
					context.Dispose();
				}
			}
		}
	}
}
