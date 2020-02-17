﻿using System;
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

				bool hasNamedEntityUpdate = false;
				if (feed.LiveData?.Boxscore?.Teams?.Away?.Coaches != null)
				{
					var awayCoachName = feed.LiveData.Boxscore.Teams.Away.Coaches.SingleOrDefault(x => x.Position.Code == "HC")?.Person?.FullName;
					if (awayCoachName != dbGame.AwayCoachName)
					{
						dbGame.AwayCoachName = awayCoachName;
						hasNamedEntityUpdate = true;
					}
				}
				if (feed.LiveData?.Boxscore?.Teams?.Home?.Coaches != null)
				{
					var homeCoachName = feed.LiveData.Boxscore.Teams.Home.Coaches.SingleOrDefault(x => x.Position.Code == "HC")?.Person?.FullName;
					if (homeCoachName != dbGame.HomeCoachName)
					{
						dbGame.HomeCoachName = homeCoachName;
						hasNamedEntityUpdate = true;
					}
				}

				string refereeName1 = null, refereeName2 = null;
				string linesmanName1 = null, linesmanName2 = null;
				var apiOfficials = feed.LiveData?.Boxscore?.Officials.ToList();
				if (apiOfficials != null)
				{
					var referees = apiOfficials.Where(x => x.OfficialType == "Referee").ToList();
					if (referees.Count >= 1) { refereeName1 = referees[0].Official.FullName; }
					if (referees.Count >= 2) { refereeName2 = referees[1].Official.FullName; }
					var linesmen = apiOfficials.Where(x => x.OfficialType == "Linesman").ToList();
					if (linesmen.Count >= 1) { linesmanName1 = linesmen[0].Official.FullName; }
					if (linesmen.Count >= 2) { linesmanName2 = linesmen[1].Official.FullName; }

					if ((refereeName1 == null && dbGame.RefereeName1 != null) || (refereeName1 != null && dbGame.RefereeName1 != refereeName1))
					{
						dbGame.RefereeName1 = refereeName1;
						hasNamedEntityUpdate = true;
					}
					if ((refereeName2 == null && dbGame.RefereeName2 != null) || (refereeName2 != null && dbGame.RefereeName2 != refereeName2))
					{
						dbGame.RefereeName2 = refereeName2;
						hasNamedEntityUpdate = true;
					}

					if ((linesmanName1 == null && dbGame.LinesmanName1 != null) || (linesmanName1 != null && dbGame.LinesmanName1 != linesmanName1))
					{
						dbGame.LinesmanName1 = linesmanName1;
						hasNamedEntityUpdate = true;
					}
					if ((linesmanName2 == null && dbGame.LinesmanName2 != null) || (linesmanName2 != null && dbGame.LinesmanName2 != linesmanName2))
					{
						dbGame.LinesmanName2 = linesmanName2;
						hasNamedEntityUpdate = true;
					}
				}

				if (hasNamedEntityUpdate)
				{
					dbContext.SaveChanges();
				}

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

					bool enableStarAwards = feed.LiveData?.Decisions?.FirstStar != null
											&& feed.LiveData.Decisions.SecondStar != null
											&& feed.LiveData.Decisions.ThirdStar != null;
					int? firstStarNhlPlayerId = null;
					int? secondStarNhlPlayerId = null;
					int? thirdStarNhlPlayerId = null;
					if (enableStarAwards)
					{
						firstStarNhlPlayerId = feed.LiveData.Decisions.FirstStar.Id;
						secondStarNhlPlayerId = feed.LiveData.Decisions.SecondStar.Id;
						thirdStarNhlPlayerId = feed.LiveData.Decisions.ThirdStar.Id;
					}

					bool hasPlayerBoxscoreUpdate = false;
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

							int? starNumber = enableStarAwards
												? firstStarNhlPlayerId.HasValue && firstStarNhlPlayerId == dbPlayer.NhlPlayerId
													? 1
													: secondStarNhlPlayerId.HasValue && secondStarNhlPlayerId == dbPlayer.NhlPlayerId
														? 2
														: thirdStarNhlPlayerId.HasValue && thirdStarNhlPlayerId == dbPlayer.NhlPlayerId
															? 3
															: 0
												: (int?)null;

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
										Takeaways = apiSkaterStats.Takeaways,
										StarNumber = starNumber
									};
									dbSkaterBoxscoreDict.Add(dbPlayerId, dbSkaterBoxscore);
									dbContext.SkaterBoxscores.Add(dbSkaterBoxscore);
									hasPlayerBoxscoreUpdate = true;
								}
								else if (HasUpdates(dbSkaterBoxscore, apiSkaterStats, starNumber))
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
									dbSkaterBoxscore.StarNumber = starNumber;
									hasPlayerBoxscoreUpdate = true;
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
										TimeOnIce = ConvertTimeStringToSeconds(apiGoalieStats.TimeOnIce),
										StarNumber = starNumber
									};
									dbGoalieBoxscoreDict.Add(dbPlayerId, dbGoalieBoxscore);
									dbContext.GoalieBoxscores.Add(dbGoalieBoxscore);
									hasPlayerBoxscoreUpdate = true;
								}
								else if (HasUpdates(dbGoalieBoxscore, apiGoalieStats, starNumber))
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
									dbGoalieBoxscore.StarNumber = starNumber;
									hasPlayerBoxscoreUpdate = true;
								}
							}
						}

						#region TEAM BOXSCORES
						bool hasTeamBoxscoreUpdate = false;
						var dbTeamBoxscores = dbContext.TeamBoxscores.Where(x => x.GameId == dbGameId).ToList();
						var apiTeamBoxscoreTeams = new[] { feed.LiveData.Boxscore.Teams.Away, feed.LiveData.Boxscore.Teams.Home };
						foreach (var apiTeamBoxscoreTeam in apiTeamBoxscoreTeams)
						{
							var apiTeamBoxscore = apiTeamBoxscoreTeam.TeamStats.TeamSkaterStats;
							var isHome = feed.GameData.Teams.Home.Id == apiTeamBoxscoreTeam.Team.Id;
							var dbTeamId = isHome ? dbHomeTeamId : dbAwayTeamId;
							var dbOppTeamId = isHome ? dbAwayTeamId : dbHomeTeamId;
							var dbTeamBox = dbTeamBoxscores.SingleOrDefault(x => x.TeamId == dbTeamId);
							var dbOppTeamBox = dbTeamBoxscores.SingleOrDefault(x => x.TeamId == dbOppTeamId);

							int? faceoffsTaken = apiTeamBoxscoreTeam.Players?.Values.Sum(x => x.Stats?.SkaterStats?.FaceoffTaken);
							int? faceoffsWon = apiTeamBoxscoreTeam.Players?.Values.Sum(x => x.Stats?.SkaterStats?.FaceOffWins);
							int? iceTimeEV = apiTeamBoxscoreTeam.Players?.Values.Sum(x => ConvertTimeStringToSeconds(x.Stats?.SkaterStats?.EvenTimeOnIce));
							int? iceTimePP = apiTeamBoxscoreTeam.Players?.Values.Sum(x => ConvertTimeStringToSeconds(x.Stats?.SkaterStats?.PowerPlayTimeOnIce));
							int? iceTimeSH = apiTeamBoxscoreTeam.Players?.Values.Sum(x => ConvertTimeStringToSeconds(x.Stats?.SkaterStats?.ShortHandedTimeOnIce));
							int? iceTimeTotal = apiTeamBoxscoreTeam.Players?.Values.Sum(x => ConvertTimeStringToSeconds(x.Stats?.SkaterStats?.TimeOnIce));
							int? goalsPP = apiTeamBoxscoreTeam.Players?.Values.Sum(x => x.Stats?.SkaterStats?.PowerPlayGoals);
							int? goalsEV = apiTeamBoxscoreTeam.Players?.Values.Sum(x => x.Stats?.SkaterStats?.ShortHandedGoals);
							int? goalsSH = apiTeamBoxscoreTeam.Players?.Values.Sum(x =>
							{
								var apiSkaterStats = x.Stats?.SkaterStats;
								if (apiSkaterStats?.Goals == null || !apiSkaterStats.PowerPlayGoals.HasValue || !apiSkaterStats.ShortHandedGoals.HasValue)
								{
									return null;
								}
								return apiSkaterStats.Goals - apiSkaterStats.PowerPlayGoals - apiSkaterStats.ShortHandedGoals;
							});

							if (dbTeamBox == null)
							{
								dbTeamBox = new TeamBoxscore
								{
									GameId = dbGameId,
									TeamId = dbTeamId,
									OppTeamId = dbOppTeamId,
									IsHome = isHome,
									FaceoffsTaken = faceoffsTaken,
									FaceoffsWon = faceoffsWon,
									Giveaways = apiTeamBoxscore.Giveaways,
									Goals = apiTeamBoxscore.Goals,
									GoalsPP = goalsPP,
									GoalsSH = goalsEV,
									GoalsEV = goalsSH,
									Hits = apiTeamBoxscore.Hits,
									IceTimeEV = iceTimeEV,
									IceTimePP = iceTimePP,
									IceTimeSH = iceTimeSH,
									IceTimeTotal = iceTimeTotal,
									OppShotsBlocked = apiTeamBoxscore.Blocked,
									PenaltyMinutes = apiTeamBoxscore.Pim,
									PowerPlayOpps = (int?)apiTeamBoxscore.PowerPlayOpportunities,
									Shots = apiTeamBoxscore.Shots,
									Takeaways = apiTeamBoxscore.Takeaways
								};
								dbContext.TeamBoxscores.Add(dbTeamBox);
								dbTeamBoxscores.Add(dbTeamBox);
								hasTeamBoxscoreUpdate = true;
							}
							else if (dbTeamBox.FaceoffsTaken != faceoffsTaken
										|| dbTeamBox.FaceoffsWon != faceoffsWon
										|| dbTeamBox.Giveaways != apiTeamBoxscore.Giveaways
										|| dbTeamBox.Goals != apiTeamBoxscore.Goals
										|| dbTeamBox.GoalsPP != goalsPP
										|| dbTeamBox.GoalsSH != goalsEV
										|| dbTeamBox.GoalsEV != goalsSH
										|| dbTeamBox.Hits != apiTeamBoxscore.Hits
										|| dbTeamBox.IceTimeEV != iceTimeEV
										|| dbTeamBox.IceTimePP != iceTimePP
										|| dbTeamBox.IceTimeSH != iceTimeSH
										|| dbTeamBox.IceTimeTotal != iceTimeTotal
										|| dbTeamBox.OppShotsBlocked != apiTeamBoxscore.Blocked
										|| dbTeamBox.PenaltyMinutes != apiTeamBoxscore.Pim
										|| dbTeamBox.PowerPlayOpps != (int?)apiTeamBoxscore.PowerPlayOpportunities
										|| dbTeamBox.Shots != apiTeamBoxscore.Shots
										|| dbTeamBox.Takeaways != apiTeamBoxscore.Takeaways
								)
							{
								dbTeamBox.FaceoffsTaken = faceoffsTaken;
								dbTeamBox.FaceoffsWon = faceoffsWon;
								dbTeamBox.Giveaways = apiTeamBoxscore.Giveaways;
								dbTeamBox.Goals = apiTeamBoxscore.Goals;
								dbTeamBox.GoalsPP = goalsPP;
								dbTeamBox.GoalsSH = goalsEV;
								dbTeamBox.GoalsEV = goalsSH;
								dbTeamBox.Hits = apiTeamBoxscore.Hits;
								dbTeamBox.IceTimeEV = iceTimeEV;
								dbTeamBox.IceTimePP = iceTimePP;
								dbTeamBox.IceTimeSH = iceTimeSH;
								dbTeamBox.IceTimeTotal = iceTimeTotal;
								dbTeamBox.OppShotsBlocked = apiTeamBoxscore.Blocked;
								dbTeamBox.PenaltyMinutes = apiTeamBoxscore.Pim;
								dbTeamBox.PowerPlayOpps = (int?)apiTeamBoxscore.PowerPlayOpportunities;
								dbTeamBox.Shots = apiTeamBoxscore.Shots;
								dbTeamBox.Takeaways = apiTeamBoxscore.Takeaways;
								hasTeamBoxscoreUpdate = true;
							}
						}
						#endregion TEAM BOXSCORES

						if (hasPlayerBoxscoreUpdate || hasTeamBoxscoreUpdate)
						{
							dbContext.SaveChanges();
						}
					}
				}
			}
		}

		private bool HasUpdates(SkaterBoxscore dbBoxscore, Feeds.GameLiveFeed.ApiSkaterStats apiBoxscore, int? starNumber)
		{
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
					|| dbBoxscore.IceTimeSH != ConvertTimeStringToSeconds(apiBoxscore.ShortHandedTimeOnIce)
					|| dbBoxscore.StarNumber != starNumber;
		}

		private bool HasUpdates(GoalieBoxscore dbBoxscore, Feeds.GameLiveFeed.ApiGoalieStats apiBoxscore, int? starNumber)
		{
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
					|| dbBoxscore.TimeOnIce != ConvertTimeStringToSeconds(apiBoxscore.TimeOnIce)
					|| dbBoxscore.StarNumber != starNumber;
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
