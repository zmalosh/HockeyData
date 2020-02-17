using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HockeyData.Model;

namespace HockeyData.Processors.NhlCom.Processors
{
	public class GameLiveProcessor : IProcessor
	{
		private readonly int NhlGameId;
		private readonly JsonUtility JsonUtility;

		public GameLiveProcessor(int nhlGameId)
		{
			this.NhlGameId = nhlGameId;
			this.JsonUtility = new JsonUtility(7 * 24 * 60 * 60);
		}

		public void Run(HockeyDataContext dbContext)
		{
			var url = Feeds.GameLiveFeed.GetFeedUrl(this.NhlGameId);
			var rawJson = this.JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.GameLiveFeed.FromJson(rawJson);


			var nhlGamePlayerIds = feed.GameData?.Players?.Select(x => x.Value.Id).Distinct().ToList();
			var dbGame = dbContext.Games.Single(x => x.NhlGameId == this.NhlGameId);

			bool isGameStarted = dbGame.GameStatusId == GameStatus.Live || dbGame.GameStatusId == GameStatus.Final;
			bool feedHasPlayers = nhlGamePlayerIds != null && nhlGamePlayerIds.Count > 0;
			if (isGameStarted && feedHasPlayers)
			{
				var playersDict = dbContext.Players.Where(x => nhlGamePlayerIds.Contains(x.NhlPlayerId)).ToDictionary(x => x.NhlPlayerId, y => y);
				var apiPlayers = feed.GameData.Players.ToList();

				foreach (var apiPlayerEntry in apiPlayers)
				{
					var apiPlayer = apiPlayerEntry.Value;

					int? height = null;
					if (!string.IsNullOrEmpty(apiPlayer.Height) && apiPlayer.Height.Contains('\''))
					{
						var arrHeight = apiPlayer.Height.Replace("\"", "").Split('\'');
						height = (int.Parse(arrHeight[0]) * 12) + int.Parse(arrHeight[1]);
					}

					if (!playersDict.TryGetValue(apiPlayer.Id, out Player dbPlayer))
					{

						dbPlayer = new Player
						{
							BirthCity = apiPlayer.BirthCity,
							BirthCountry = apiPlayer.BirthCountry,
							BirthDate = apiPlayer.BirthDate.DateTime.Date,
							BirthState = apiPlayer.BirthStateProvince,
							FirstName = apiPlayer.FirstName,
							FullName = apiPlayer.FullName,
							Handedness = apiPlayer.ShootsCatches,
							Height = height,
							IsActive = apiPlayer.Active,
							JerseyNumber = apiPlayer.PrimaryNumber,
							LastName = apiPlayer.LastName,
							Nationality = apiPlayer.Nationality,
							NhlPlayerId = apiPlayer.Id,
							PrimaryPosition = apiPlayer.PrimaryPosition?.Abbreviation,
							Weight = apiPlayer.Weight == 0 ? (int?)null : apiPlayer.Weight
						};
						playersDict.Add(dbPlayer.NhlPlayerId, dbPlayer);
						dbContext.Players.Add(dbPlayer);
					}
					else if (apiPlayer.Active != dbPlayer.IsActive
							|| height != dbPlayer.Height
							|| (apiPlayer.Weight != dbPlayer.Weight || (apiPlayer.Weight == 0 && !dbPlayer.Weight.HasValue))
							|| apiPlayer.FirstName != dbPlayer.FirstName
							|| apiPlayer.LastName != dbPlayer.LastName
							|| apiPlayer.FullName != dbPlayer.FullName
							|| apiPlayer.PrimaryNumber != dbPlayer.JerseyNumber
							|| apiPlayer.PrimaryPosition.Abbreviation != dbPlayer.PrimaryPosition
							|| apiPlayer.ShootsCatches != dbPlayer.Handedness)
					{
						dbPlayer.IsActive = apiPlayer.Active;
						dbPlayer.Height = height;
						dbPlayer.Weight = apiPlayer.Weight == 0 ? (int?)null : apiPlayer.Weight;
						dbPlayer.FirstName = apiPlayer.FirstName;
						dbPlayer.LastName = apiPlayer.LastName;
						dbPlayer.FullName = apiPlayer.FullName;
						dbPlayer.JerseyNumber = apiPlayer.PrimaryNumber;
						dbPlayer.PrimaryPosition = apiPlayer.PrimaryPosition.Abbreviation;
						dbPlayer.Handedness = apiPlayer.ShootsCatches;
					}
				}
				dbContext.SaveChanges();
				playersDict = dbContext.Players.Where(x => nhlGamePlayerIds.Contains(x.NhlPlayerId)).ToDictionary(x => x.NhlPlayerId, y => y);

				int dbGameId = dbGame.GameId;
				int dbHomeTeamId = dbGame.HomeTeamId.Value;
				int dbAwayTeamId = dbGame.AwayTeamId.Value;

				if (feed.LiveData.Boxscore?.Teams?.Away?.Players != null && feed.LiveData.Boxscore.Teams.Home.Players != null)
				{
					var dbSkaterBoxscoreDict = dbContext.SkaterBoxscores.Where(x => x.GameId == dbGameId).ToDictionary(x => x.PlayerId, y => y);
					var apiPlayerBoxscores = feed.LiveData.Boxscore.Teams.Away.Players.Select(x => new { DbTeamId = dbAwayTeamId, Box = x.Value }).ToList();
					apiPlayerBoxscores.AddRange(feed.LiveData.Boxscore.Teams.Home.Players.Select(x => new { DbTeamId = dbHomeTeamId, Box = x.Value }));

					foreach (var apiPlayerBoxscore in apiPlayerBoxscores)
					{
						var dbPlayer = playersDict[apiPlayerBoxscore.Box.Person.Id];
						int dbPlayerId = dbPlayer.PlayerId;
						if (apiPlayerBoxscore.Box?.Stats?.SkaterStats != null)
						{
							var apiSkaterStats = apiPlayerBoxscore.Box.Stats.SkaterStats;
							if (!dbSkaterBoxscoreDict.TryGetValue(dbPlayerId, out SkaterBoxscore dbSkaterBoxscore))
							{
								dbSkaterBoxscore = new SkaterBoxscore
								{
									GameId = dbGameId,
									PlayerId = dbPlayerId,
									TeamId = apiPlayerBoxscore.DbTeamId,
									Assists = apiSkaterStats.Assists,
									AssistsPP = apiSkaterStats.PowerPlayAssists,
									AssistsSH = apiSkaterStats.ShortHandedAssists,
									FaceoffsTaken = apiSkaterStats.FaceoffTaken,
									FaceoffsWon = apiSkaterStats.FaceOffWins,
									Giveaways = apiSkaterStats.Giveaways,
									Goals = apiSkaterStats.Goals,
									GoalsPP = apiSkaterStats.PowerPlayGoals,
									GoalsSH = apiSkaterStats.ShortHandedGoals,
									Hits = apiSkaterStats.Hits,
									IceTimeEV = ConvertTimeStringToSeconds(apiSkaterStats.EvenTimeOnIce),
									IceTimePP = ConvertTimeStringToSeconds(apiSkaterStats.PowerPlayTimeOnIce),
									IceTimeSH = ConvertTimeStringToSeconds(apiSkaterStats.ShortHandedTimeOnIce),
									IceTimeTotal = ConvertTimeStringToSeconds(apiSkaterStats.TimeOnIce),
									JerseyNumber = apiPlayerBoxscore.Box.JerseyNumber,
									OppShotsBlocked = apiSkaterStats.Blocked,
									PenaltyMinutes = apiSkaterStats.PenaltyMinutes,
									PlusMinus = apiSkaterStats.PlusMinus,
									Position = apiPlayerBoxscore.Box.Position.Abbreviation,
									Shots = apiSkaterStats.Shots,
									Takeaways = apiSkaterStats.Takeaways
								};
								dbSkaterBoxscoreDict.Add(dbPlayerId, dbSkaterBoxscore);
								dbContext.SkaterBoxscores.Add(dbSkaterBoxscore);
							}
							else if (HasUpdates(dbSkaterBoxscore, apiSkaterStats))
							{
								dbSkaterBoxscore.Assists = apiSkaterStats.Assists;
								dbSkaterBoxscore.AssistsPP = apiSkaterStats.PowerPlayAssists;
								dbSkaterBoxscore.AssistsSH = apiSkaterStats.ShortHandedAssists;
								dbSkaterBoxscore.FaceoffsTaken = apiSkaterStats.FaceoffTaken;
								dbSkaterBoxscore.FaceoffsWon = apiSkaterStats.FaceOffWins;
								dbSkaterBoxscore.Giveaways = apiSkaterStats.Giveaways;
								dbSkaterBoxscore.Goals = apiSkaterStats.Goals;
								dbSkaterBoxscore.GoalsPP = apiSkaterStats.PowerPlayGoals;
								dbSkaterBoxscore.GoalsSH = apiSkaterStats.ShortHandedGoals;
								dbSkaterBoxscore.Hits = apiSkaterStats.Hits;
								dbSkaterBoxscore.IceTimeEV = ConvertTimeStringToSeconds(apiSkaterStats.EvenTimeOnIce);
								dbSkaterBoxscore.IceTimePP = ConvertTimeStringToSeconds(apiSkaterStats.PowerPlayTimeOnIce);
								dbSkaterBoxscore.IceTimeSH = ConvertTimeStringToSeconds(apiSkaterStats.ShortHandedTimeOnIce);
								dbSkaterBoxscore.IceTimeTotal = ConvertTimeStringToSeconds(apiSkaterStats.TimeOnIce);
								dbSkaterBoxscore.OppShotsBlocked = apiSkaterStats.Blocked;
								dbSkaterBoxscore.PenaltyMinutes = apiSkaterStats.PenaltyMinutes;
								dbSkaterBoxscore.PlusMinus = apiSkaterStats.PlusMinus;
								dbSkaterBoxscore.Shots = apiSkaterStats.Shots;
								dbSkaterBoxscore.Takeaways = apiSkaterStats.Takeaways;
							}
						}
					}
					dbContext.SaveChanges();
				}
			}
		}

		private bool HasUpdates(SkaterBoxscore dbBoxscore, Feeds.GameLiveFeed.ApiSkaterStats apiBoxscore)
		{
			return dbBoxscore.Assists != apiBoxscore.Assists
			|| dbBoxscore.AssistsPP != apiBoxscore.PowerPlayAssists
			|| dbBoxscore.AssistsSH != apiBoxscore.ShortHandedAssists
			|| dbBoxscore.FaceoffsTaken != apiBoxscore.FaceoffTaken
			|| dbBoxscore.FaceoffsWon != apiBoxscore.FaceOffWins
			|| dbBoxscore.Giveaways != apiBoxscore.Giveaways
			|| dbBoxscore.Goals != apiBoxscore.Goals
			|| dbBoxscore.GoalsPP != apiBoxscore.PowerPlayGoals
			|| dbBoxscore.GoalsSH != apiBoxscore.ShortHandedGoals
			|| dbBoxscore.Hits != apiBoxscore.Hits
			|| dbBoxscore.IceTimeEV != ConvertTimeStringToSeconds(apiBoxscore.EvenTimeOnIce)
			|| dbBoxscore.IceTimePP != ConvertTimeStringToSeconds(apiBoxscore.PowerPlayTimeOnIce)
			|| dbBoxscore.IceTimeSH != ConvertTimeStringToSeconds(apiBoxscore.ShortHandedTimeOnIce)
			|| dbBoxscore.IceTimeTotal != ConvertTimeStringToSeconds(apiBoxscore.TimeOnIce)
			|| dbBoxscore.OppShotsBlocked != apiBoxscore.Blocked
			|| dbBoxscore.PenaltyMinutes != apiBoxscore.PenaltyMinutes
			|| dbBoxscore.PlusMinus != apiBoxscore.PlusMinus
			|| dbBoxscore.Shots != apiBoxscore.Shots
			|| dbBoxscore.Takeaways != apiBoxscore.Takeaways;
		}

		private static int? ConvertHeightStringToInches(string height)
		{
			if (string.IsNullOrEmpty(height) || !height.Contains('\''))
			{
				return null;
			}
			var arrHeight = height.Replace("\"", "").Split('\'');
			int? result = (int.Parse(arrHeight[0]) * 12) + int.Parse(arrHeight[1]);
			return result;
		}

		private static int? ConvertTimeStringToSeconds(string time)
		{
			if (string.IsNullOrEmpty(time) || !time.Contains(':'))
			{
				return null;
			}
			var arrTime = time.Split(':');
			int? result = (int.Parse(arrTime[0]) * 60) + int.Parse(arrTime[1]);
			return result;
		}
	}
}
