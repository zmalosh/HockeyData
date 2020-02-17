using HockeyData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyData.Processors.NhlCom.Processors
{
	public class GamesProcessor : IProcessor
	{
		private readonly string nhlSeasonKey;

		public GamesProcessor(string nhlSeasonKey = null)
		{
			this.nhlSeasonKey = nhlSeasonKey;
		}

		public void Run(HockeyDataContext dbContext)
		{
			var url = Feeds.GamesFeed.GetFeedUrl(this.nhlSeasonKey);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.GamesFeed.FromJson(rawJson);

			int dbSeasonId;
			if (this.nhlSeasonKey == null)
			{
				dbSeasonId = dbContext.Seasons.OrderByDescending(x => x.NhlSeasonKey).First().SeasonId;
			}
			else
			{
				dbSeasonId = dbContext.Seasons.Single(x => x.NhlSeasonKey == this.nhlSeasonKey).SeasonId;
			}

			var teamsDict = dbContext.Teams.ToDictionary(x => x.NhlTeamId, y => y);
			var apiTeams = feed.Dates.SelectMany(x => x.Games.SelectMany(y => new[] { y.Teams?.Home?.Team, y.Teams?.Away?.Team })).Distinct().ToList();
			foreach (var apiTeam in apiTeams)
			{
				if (apiTeam != null && !teamsDict.TryGetValue(apiTeam.Id.Value, out Team dbTeam))
				{
					dbTeam = new Team
					{
						NhlTeamId = apiTeam.Id.Value,
						TeamFullName = apiTeam.Name
					};
					teamsDict.Add(apiTeam.Id.Value, dbTeam);
					dbContext.Teams.Add(dbTeam);
					dbContext.SaveChanges();
				}
			}

			teamsDict = dbContext.Teams.ToDictionary(x => x.NhlTeamId, y => y);
			var gameTypesDict = dbContext.RefGameTypes.ToDictionary(x => x.NhlGameTypeKey, y => y.GameTypeId);
			var gameStatusesDict = dbContext.RefGameStatuses.ToDictionary(x => x.NhlStatusCode, y => y.GameStatusId);
			var detailedGameStatusesDict = dbContext.RefGameStatuses.ToDictionary(x => x.NhlStatusCode, y => y.DetailedGameStatusId);
			var gamesDict = dbContext.Games.Where(x => nhlSeasonKey == null || x.Season.NhlSeasonKey == this.nhlSeasonKey).ToDictionary(x => x.NhlGameId, y => y);

			var apiGames = feed.Dates.SelectMany(x => x.Games).OrderBy(x => x.GameDate).ToList();

			foreach (var apiGame in apiGames)
			{
				var apiDetailedStatus = detailedGameStatusesDict[apiGame.Status.StatusCode];
				var apiStatus = gameStatusesDict[apiGame.Status.StatusCode];
				int? apiHomeGamesPlayed = GetGamesPlayed(apiGame.Teams?.Home?.LeagueRecord);
				int? apiAwayGamesPlayed = GetGamesPlayed(apiGame.Teams?.Away?.LeagueRecord);
				var homeScore = apiGame.Teams?.Home?.Score;
				var awayScore = apiGame.Teams?.Away?.Score;
				var gameTimeUtc = apiGame.GameDate.UtcDateTime;
				var homeTeamId = apiGame.Teams?.Home?.Team == null ? (int?)null : teamsDict[apiGame.Teams.Home.Team.Id.Value].TeamId;
				var awayTeamId = apiGame.Teams?.Away?.Team == null ? (int?)null : teamsDict[apiGame.Teams.Away.Team.Id.Value].TeamId;

				if (!gamesDict.TryGetValue(apiGame.GamePk, out Game dbGame))
				{
					dbGame = new Game
					{
						NhlGameId = apiGame.GamePk,
						GameTimeUtc = gameTimeUtc,
						GameDateEst = gameTimeUtc.AddHours(-5).Date,
						SeasonId = dbSeasonId,
						HomeTeamId = homeTeamId,
						AwayTeamId = awayTeamId,
						HomeScore = homeScore,
						AwayScore = awayScore,
						NhlVenueId = apiGame.Venue?.Id,
						VenueName = apiGame.Venue?.Name,
						DetailedGameStatusId = apiDetailedStatus,
						GameStatusId = apiStatus,
						GameTypeId = gameTypesDict[apiGame.GameType],
						AwayGamesPlayed = apiAwayGamesPlayed,
						AwayLosses = apiGame.Teams?.Away?.LeagueRecord.Losses,
						AwayWins = apiGame.Teams?.Away?.LeagueRecord.Wins,
						AwayTies = apiGame.Teams?.Away?.LeagueRecord.Ties,
						AwayOvertimes = apiGame.Teams?.Away?.LeagueRecord.Overtimes,
						HomeGamesPlayed = apiHomeGamesPlayed,
						HomeLosses = apiGame.Teams?.Home?.LeagueRecord.Losses,
						HomeWins = apiGame.Teams?.Home?.LeagueRecord.Wins,
						HomeTies = apiGame.Teams?.Home?.LeagueRecord.Ties,
						HomeOvertimes = apiGame.Teams?.Home?.LeagueRecord.Overtimes,
					};
					gamesDict.Add(dbGame.NhlGameId, dbGame);
					dbContext.Games.Add(dbGame);
				}
				else if (
					dbGame.DetailedGameStatusId != apiDetailedStatus
					|| dbGame.GameStatusId != apiStatus
					|| dbGame.HomeScore != homeScore
					|| dbGame.AwayScore != awayScore
					|| dbGame.HomeGamesPlayed != apiHomeGamesPlayed
					|| dbGame.AwayGamesPlayed != apiAwayGamesPlayed
					|| dbGame.GameTimeUtc != gameTimeUtc
					|| dbGame.HomeTeamId != homeTeamId
					|| dbGame.AwayTeamId != awayTeamId
				)
				{
					dbGame.DetailedGameStatusId = apiDetailedStatus;
					dbGame.GameStatusId = apiStatus;
					dbGame.HomeScore = homeScore;
					dbGame.AwayScore = awayScore;
					dbGame.HomeGamesPlayed = apiHomeGamesPlayed;
					dbGame.AwayGamesPlayed = apiAwayGamesPlayed;
					dbGame.GameTimeUtc = gameTimeUtc;
					dbGame.HomeTeamId = homeTeamId;
					dbGame.AwayTeamId = awayTeamId;
				}
			}
			dbContext.SaveChanges();
		}

		private int? GetGamesPlayed(Feeds.GamesFeed.ApiLeagueRecord teamRecord)
		{
			if (teamRecord == null) { return null; }
			return teamRecord.Losses
				+ teamRecord.Wins
				+ teamRecord.Overtimes ?? 0
				+ teamRecord.Ties ?? 0;
		}
	}
}
