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

			if (!string.IsNullOrEmpty(rawJson))
			{
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
						var apiPlayerBoxscores = feed.LiveData.Boxscore.Teams.Away.Players.Select(x => new { DbTeamId = dbAwayTeamId, Box = x.Value }).ToList();
						apiPlayerBoxscores.AddRange(feed.LiveData.Boxscore.Teams.Home.Players.Select(x => new { DbTeamId = dbHomeTeamId, Box = x.Value }));

						var dbSkaterBoxscoreDict = dbContext.SkaterBoxscores.Where(x => x.GameId == dbGameId).ToDictionary(x => x.PlayerId, y => y);
						var dbGoalieBoxscoreDict = dbContext.GoalieBoxscores.Where(x => x.GameId == dbGameId).ToDictionary(x => x.PlayerId, y => y);

						foreach (var apiPlayerBoxscore in apiPlayerBoxscores)
						{
							var dbPlayer = playersDict[apiPlayerBoxscore.Box.Person.Id];
							int dbPlayerId = dbPlayer.PlayerId;
							if (apiPlayerBoxscore.Box?.Stats?.SkaterStats != null)
							{
								var apiSkaterStats = apiPlayerBoxscore.Box.Stats.SkaterStats;
								int? assistsEV = (apiSkaterStats.Assists.HasValue && apiSkaterStats.PowerPlayAssists.HasValue && apiSkaterStats.ShortHandedAssists.HasValue)
													? apiSkaterStats.Assists - apiSkaterStats.PowerPlayAssists - apiSkaterStats.ShortHandedAssists
													: null;
								int? goalsEV = (apiSkaterStats.Goals.HasValue && apiSkaterStats.PowerPlayGoals.HasValue && apiSkaterStats.ShortHandedGoals.HasValue)
													? apiSkaterStats.Goals - apiSkaterStats.PowerPlayGoals - apiSkaterStats.ShortHandedGoals
													: null;
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
										AssistsEV = assistsEV,
										FaceoffsTaken = apiSkaterStats.FaceoffTaken,
										FaceoffsWon = apiSkaterStats.FaceOffWins,
										Giveaways = apiSkaterStats.Giveaways,
										Goals = apiSkaterStats.Goals,
										GoalsPP = apiSkaterStats.PowerPlayGoals,
										GoalsSH = apiSkaterStats.ShortHandedGoals,
										GoalsEV = goalsEV,
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
									dbSkaterBoxscore.AssistsEV = assistsEV;
									dbSkaterBoxscore.FaceoffsTaken = apiSkaterStats.FaceoffTaken;
									dbSkaterBoxscore.FaceoffsWon = apiSkaterStats.FaceOffWins;
									dbSkaterBoxscore.Giveaways = apiSkaterStats.Giveaways;
									dbSkaterBoxscore.Goals = apiSkaterStats.Goals;
									dbSkaterBoxscore.GoalsPP = apiSkaterStats.PowerPlayGoals;
									dbSkaterBoxscore.GoalsSH = apiSkaterStats.ShortHandedGoals;
									dbSkaterBoxscore.GoalsEV = goalsEV;
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

							if (apiPlayerBoxscore.Box?.Stats?.GoalieStats != null)
							{
								var apiGoalieStats = apiPlayerBoxscore.Box.Stats.GoalieStats;
								int? goalsAllowedEV = apiGoalieStats.EvenShotsAgainst.HasValue && apiGoalieStats.EvenSaves.HasValue
															? apiGoalieStats.EvenShotsAgainst - apiGoalieStats.EvenSaves
															: null;
								int? goalsAllowedPP = apiGoalieStats.PowerPlayShotsAgainst.HasValue && apiGoalieStats.PowerPlaySaves.HasValue
															? apiGoalieStats.PowerPlayShotsAgainst - apiGoalieStats.PowerPlaySaves
															: null;
								int? goalsAllowedSH = apiGoalieStats.ShortHandedShotsAgainst.HasValue && apiGoalieStats.ShortHandedSaves.HasValue
															? apiGoalieStats.ShortHandedShotsAgainst - apiGoalieStats.ShortHandedSaves
															: null;
								if (!dbGoalieBoxscoreDict.TryGetValue(dbPlayerId, out GoalieBoxscore dbGoalieBoxscore))
								{
									dbGoalieBoxscore = new GoalieBoxscore
									{
										GameId = dbGameId,
										PlayerId = dbPlayerId,
										TeamId = apiPlayerBoxscore.DbTeamId,
										AssistsScored = apiGoalieStats.Assists,
										Decision = string.IsNullOrEmpty(apiGoalieStats.Decision) ? null : apiGoalieStats.Decision,
										GoalsAllowed = apiGoalieStats.ShotsAgainst - apiGoalieStats.Saves,
										GoalsAllowedEV = goalsAllowedEV,
										GoalsAllowedPP = goalsAllowedPP,
										GoalsAllowedSH = goalsAllowedSH,
										GoalsScored = apiGoalieStats.Goals,
										JerseyNumber = apiPlayerBoxscore.Box.JerseyNumber,
										PenaltyMinutes = apiGoalieStats.Pim,
										Saves = apiGoalieStats.Saves,
										SavesEV = apiGoalieStats.EvenSaves,
										SavesPP = apiGoalieStats.PowerPlaySaves,
										SavesSH = apiGoalieStats.ShortHandedSaves,
										Shots = apiGoalieStats.ShotsAgainst,
										ShotsEV = apiGoalieStats.EvenShotsAgainst,
										ShotsPP = apiGoalieStats.PowerPlayShotsAgainst,
										ShotsSH = apiGoalieStats.ShortHandedShotsAgainst,
										TimeOnIce = ConvertTimeStringToSeconds(apiGoalieStats.TimeOnIce)
									};
									dbGoalieBoxscoreDict.Add(dbPlayerId, dbGoalieBoxscore);
									dbContext.GoalieBoxscores.Add(dbGoalieBoxscore);
								}
								else if (HasUpdates(dbGoalieBoxscore, apiGoalieStats))
								{
									dbGoalieBoxscore.AssistsScored = apiGoalieStats.Assists;
									dbGoalieBoxscore.Decision = string.IsNullOrEmpty(apiGoalieStats.Decision) ? null : apiGoalieStats.Decision;
									dbGoalieBoxscore.GoalsAllowed = apiGoalieStats.ShotsAgainst - apiGoalieStats.Saves;
									dbGoalieBoxscore.GoalsAllowedEV = goalsAllowedEV;
									dbGoalieBoxscore.GoalsAllowedPP = goalsAllowedPP;
									dbGoalieBoxscore.GoalsAllowedSH = goalsAllowedSH;
									dbGoalieBoxscore.GoalsScored = apiGoalieStats.Goals;
									dbGoalieBoxscore.JerseyNumber = apiPlayerBoxscore.Box.JerseyNumber;
									dbGoalieBoxscore.PenaltyMinutes = apiGoalieStats.Pim;
									dbGoalieBoxscore.Saves = apiGoalieStats.Saves;
									dbGoalieBoxscore.SavesEV = apiGoalieStats.EvenSaves;
									dbGoalieBoxscore.SavesPP = apiGoalieStats.PowerPlaySaves;
									dbGoalieBoxscore.SavesSH = apiGoalieStats.ShortHandedSaves;
									dbGoalieBoxscore.Shots = apiGoalieStats.ShotsAgainst;
									dbGoalieBoxscore.ShotsEV = apiGoalieStats.EvenShotsAgainst;
									dbGoalieBoxscore.ShotsPP = apiGoalieStats.PowerPlayShotsAgainst;
									dbGoalieBoxscore.ShotsSH = apiGoalieStats.ShortHandedShotsAgainst;
									dbGoalieBoxscore.TimeOnIce = ConvertTimeStringToSeconds(apiGoalieStats.TimeOnIce);
								}
							}
						}

						dbContext.SaveChanges();
					}
				}
			}
		}

		private bool HasUpdates(SkaterBoxscore dbBoxscore, Feeds.GameLiveFeed.ApiSkaterStats apiBoxscore)
		{
			return false;
			return dbBoxscore.IceTimeTotal != ConvertTimeStringToSeconds(apiBoxscore.TimeOnIce)
				|| dbBoxscore.Shots != apiBoxscore.Shots
				|| dbBoxscore.Giveaways != apiBoxscore.Giveaways
				|| dbBoxscore.Takeaways != apiBoxscore.Takeaways
				|| dbBoxscore.OppShotsBlocked != apiBoxscore.Blocked
				|| dbBoxscore.Hits != apiBoxscore.Hits
				|| dbBoxscore.PenaltyMinutes != apiBoxscore.PenaltyMinutes
				|| dbBoxscore.PlusMinus != apiBoxscore.PlusMinus
				|| dbBoxscore.Assists != apiBoxscore.Assists
				|| dbBoxscore.Goals != apiBoxscore.Goals
				|| dbBoxscore.FaceoffsTaken != apiBoxscore.FaceoffTaken
				|| dbBoxscore.FaceoffsWon != apiBoxscore.FaceOffWins
				|| dbBoxscore.IceTimeEV != ConvertTimeStringToSeconds(apiBoxscore.EvenTimeOnIce)
				|| dbBoxscore.AssistsPP != apiBoxscore.PowerPlayAssists
				|| dbBoxscore.AssistsSH != apiBoxscore.ShortHandedAssists
				|| dbBoxscore.GoalsPP != apiBoxscore.PowerPlayGoals
				|| dbBoxscore.GoalsSH != apiBoxscore.ShortHandedGoals
				|| dbBoxscore.IceTimePP != ConvertTimeStringToSeconds(apiBoxscore.PowerPlayTimeOnIce)
				|| dbBoxscore.IceTimeSH != ConvertTimeStringToSeconds(apiBoxscore.ShortHandedTimeOnIce);
		}

		private bool HasUpdates(GoalieBoxscore dbBoxscore, Feeds.GameLiveFeed.ApiGoalieStats apiBoxscore)
		{
			return false;
			return dbBoxscore.AssistsScored != apiBoxscore.Assists
					|| dbBoxscore.Decision != (string.IsNullOrEmpty(apiBoxscore.Decision) ? null : apiBoxscore.Decision)
					|| dbBoxscore.GoalsAllowed != apiBoxscore.ShotsAgainst - apiBoxscore.Saves
					|| dbBoxscore.GoalsScored != apiBoxscore.Goals
					|| dbBoxscore.PenaltyMinutes != apiBoxscore.Pim
					|| dbBoxscore.Saves != apiBoxscore.Saves
					|| dbBoxscore.SavesEV != apiBoxscore.EvenSaves
					|| dbBoxscore.SavesPP != apiBoxscore.PowerPlaySaves
					|| dbBoxscore.SavesSH != apiBoxscore.ShortHandedSaves
					|| dbBoxscore.Shots != apiBoxscore.ShotsAgainst
					|| dbBoxscore.ShotsEV != apiBoxscore.EvenShotsAgainst
					|| dbBoxscore.ShotsPP != apiBoxscore.PowerPlayShotsAgainst
					|| dbBoxscore.ShotsSH != apiBoxscore.ShortHandedShotsAgainst
					|| dbBoxscore.TimeOnIce != ConvertTimeStringToSeconds(apiBoxscore.TimeOnIce);
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
