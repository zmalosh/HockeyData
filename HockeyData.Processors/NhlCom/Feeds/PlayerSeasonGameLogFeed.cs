using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Processors.NhlCom.Feeds
{
	public class PlayerSeasonGameLogFeed
	{
		public static string GetFeedUrl(int nhlPlayerPk, string nhlSeasonKey)
		{
			if (string.IsNullOrEmpty(nhlSeasonKey))
			{
				return $"https://statsapi.web.nhl.com/api/v1/people/{nhlPlayerPk}/stats?stats=gameLog";
			}
			return $"https://statsapi.web.nhl.com/api/v1/people/{nhlPlayerPk}/stats?stats=gameLog&season={nhlSeasonKey}";
		}

		public static PlayerSeasonGameLogFeed FromJson(string json) => JsonConvert.DeserializeObject<PlayerSeasonGameLogFeed>(json, Converter.Settings);

		[JsonProperty("copyright")]
		public string Copyright { get; set; }

		[JsonProperty("stats")]
		public List<ApiPlayerStatElement> Stats { get; set; }

		public class ApiPlayerStatElement
		{
			[JsonProperty("type")]
			public ApiPlayerStatType Type { get; set; }

			[JsonProperty("splits")]
			public List<ApiPlayerSplit> Splits { get; set; }
		}

		public class ApiPlayerSplit
		{
			[JsonProperty("season")]
			public string Season { get; set; }

			[JsonProperty("stat")]
			public ApiPlayerSplitStat Stat { get; set; }

			[JsonProperty("team")]
			public ApiPlayerStatOpponent Team { get; set; }

			[JsonProperty("opponent")]
			public ApiPlayerStatOpponent Opponent { get; set; }

			[JsonProperty("date")]
			public DateTimeOffset Date { get; set; }

			[JsonProperty("isHome")]
			public bool IsHome { get; set; }

			[JsonProperty("isWin")]
			public bool IsWin { get; set; }

			[JsonProperty("isOT")]
			public bool IsOt { get; set; }

			[JsonProperty("game")]
			public ApiPlayerStatGame Game { get; set; }
		}

		public class ApiPlayerStatGame
		{
			[JsonProperty("gamePk")]
			public int GamePk { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("content")]
			public ApiContentLink Content { get; set; }
		}

		public class ApiContentLink
		{
			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiPlayerStatOpponent
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiPlayerSplitStat
		{
			[JsonProperty("timeOnIce")]
			public string TimeOnIce { get; set; }

			[JsonProperty("assists")]
			public int? Assists { get; set; }

			[JsonProperty("goals")]
			public int? Goals { get; set; }

			[JsonProperty("pim")]
			public int? Pim { get; set; }

			[JsonProperty("shots")]
			public int? Shots { get; set; }

			[JsonProperty("games")]
			public int? Games { get; set; }

			[JsonProperty("hits")]
			public int? Hits { get; set; }

			[JsonProperty("powerPlayGoals")]
			public int? PowerPlayGoals { get; set; }

			[JsonProperty("powerPlayPoints")]
			public int? PowerPlayPoints { get; set; }

			[JsonProperty("powerPlayTimeOnIce")]
			public string PowerPlayTimeOnIce { get; set; }

			[JsonProperty("evenTimeOnIce")]
			public string EvenTimeOnIce { get; set; }

			[JsonProperty("penaltyMinutes")]
			public int? PenaltyMinutes { get; set; }

			[JsonProperty("shotPct", NullValueHandling = NullValueHandling.Ignore)]
			public decimal? ShotPct { get; set; }

			[JsonProperty("gameWinningGoals")]
			public int? GameWinningGoals { get; set; }

			[JsonProperty("overTimeGoals")]
			public int? OverTimeGoals { get; set; }

			[JsonProperty("shortHandedGoals")]
			public int? ShortHandedGoals { get; set; }

			[JsonProperty("shortHandedPoints")]
			public int? ShortHandedPoints { get; set; }

			[JsonProperty("shortHandedTimeOnIce")]
			public string ShortHandedTimeOnIce { get; set; }

			[JsonProperty("blocked")]
			public int? Blocked { get; set; }

			[JsonProperty("plusMinus")]
			public int? PlusMinus { get; set; }

			[JsonProperty("points")]
			public int? Points { get; set; }

			[JsonProperty("shifts")]
			public int? Shifts { get; set; }

			[JsonProperty("faceOffPct", NullValueHandling = NullValueHandling.Ignore)]
			public decimal? FaceOffPct { get; set; }
		}

		public class ApiPlayerStatType
		{
			[JsonProperty("displayName")]
			public string DisplayName { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this PlayerSeasonGameLogFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
