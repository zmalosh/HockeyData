using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HockeyData.Processors.NhlCom.Feeds
{
	public class GamesFeed
	{
		public static string GetFeedUrl(string nhlSeasonKey = null)
		{
			string baseUrl = "https://statsapi.web.nhl.com/api/v1/schedule";
			if (string.IsNullOrEmpty(nhlSeasonKey))
			{
				return baseUrl;
			}
			return $"{baseUrl}?season={nhlSeasonKey}";
		}

		public static GamesFeed FromJson(string json) => JsonConvert.DeserializeObject<GamesFeed>(json, Converter.Settings);

		[JsonProperty("copyright")]
		public string Copyright { get; set; }

		[JsonProperty("totalItems")]
		public int TotalItems { get; set; }

		[JsonProperty("totalEvents")]
		public int TotalEvents { get; set; }

		[JsonProperty("totalGames")]
		public int TotalGames { get; set; }

		[JsonProperty("totalMatches")]
		public int TotalMatches { get; set; }

		[JsonProperty("wait")]
		public int Wait { get; set; }

		[JsonProperty("dates")]
		public List<Date> Dates { get; set; }

		public class Date
		{
			[JsonProperty("date")]
			public DateTime GameDate { get; set; }

			[JsonProperty("totalItems")]
			public int TotalItems { get; set; }

			[JsonProperty("totalEvents")]
			public int TotalEvents { get; set; }

			[JsonProperty("totalGames")]
			public int TotalGames { get; set; }

			[JsonProperty("totalMatches")]
			public int TotalMatches { get; set; }

			[JsonProperty("games")]
			public List<ApiGame> Games { get; set; }
		}

		public class ApiGame
		{
			[JsonProperty("gamePk")]
			public int GamePk { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("gameType")]
			public string GameType { get; set; }

			[JsonProperty("season")]
			public int Season { get; set; }

			[JsonProperty("gameDate")]
			public DateTimeOffset GameDate { get; set; }

			[JsonProperty("status")]
			public ApiGameStatus Status { get; set; }

			[JsonProperty("teams")]
			public ApiTeams Teams { get; set; }

			[JsonProperty("venue")]
			public ApiSimpleEntity Venue { get; set; }

			[JsonProperty("content")]
			public ApiContent Content { get; set; }
		}

		public class ApiContent
		{
			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiGameStatus
		{
			[JsonProperty("abstractGameState")]
			public string AbstractGameState { get; set; }

			[JsonProperty("codedGameState")]
			public int CodedGameState { get; set; }

			[JsonProperty("detailedState")]
			public string DetailedState { get; set; }

			[JsonProperty("statusCode")]
			public int StatusCode { get; set; }

			[JsonProperty("startTimeTBD")]
			public bool StartTimeTbd { get; set; }
		}

		public class ApiTeams
		{
			[JsonProperty("away")]
			public ApiTeam Away { get; set; }

			[JsonProperty("home")]
			public ApiTeam Home { get; set; }
		}

		public class ApiTeam
		{
			[JsonProperty("leagueRecord")]
			public ApiLeagueRecord LeagueRecord { get; set; }

			[JsonProperty("score")]
			public int Score { get; set; }

			[JsonProperty("team")]
			public ApiSimpleEntity Team { get; set; }
		}

		public class ApiLeagueRecord
		{
			[JsonProperty("wins")]
			public int Wins { get; set; }

			[JsonProperty("losses")]
			public int Losses { get; set; }

			[JsonProperty("ties", NullValueHandling = NullValueHandling.Ignore)]
			public int? Ties { get; set; }

			[JsonProperty("ot", NullValueHandling = NullValueHandling.Ignore)]
			public int? Overtimes { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }
		}

		public class ApiSimpleEntity : IEquatable<ApiSimpleEntity>
		{
			[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
			public int? Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			public bool Equals([AllowNull] ApiSimpleEntity other)
			{
				if (other == null)
				{
					return false;
				}
				return this.Id == other.Id;
			}
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this GamesFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
