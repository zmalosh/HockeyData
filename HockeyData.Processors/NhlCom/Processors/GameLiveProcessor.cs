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
			}
		}
	}
}
