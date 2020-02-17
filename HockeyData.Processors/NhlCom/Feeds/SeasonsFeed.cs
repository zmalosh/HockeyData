using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Processors.NhlCom.Feeds
{
	public class SeasonsFeed
	{
		public static string GetFeedUrl()
		{
			return "https://statsapi.web.nhl.com/api/v1/seasons";
		}

		public static SeasonsFeed FromJson(string json) => JsonConvert.DeserializeObject<SeasonsFeed>(json, Converter.Settings);

		[JsonProperty("copyright")]
		public string Copyright { get; set; }

		[JsonProperty("seasons")]
		public List<ApiSeason> Seasons { get; set; }

		public class ApiSeason
		{
			[JsonProperty("seasonId")]
			public string SeasonId { get; set; }

			[JsonProperty("regularSeasonStartDate")]
			public DateTime RegularSeasonStartDate { get; set; }

			[JsonProperty("regularSeasonEndDate")]
			public DateTime RegularSeasonEndDate { get; set; }

			[JsonProperty("seasonEndDate")]
			public DateTime SeasonEndDate { get; set; }

			[JsonProperty("numberOfGames")]
			public int NumberOfGames { get; set; }

			[JsonProperty("tiesInUse")]
			public bool TiesInUse { get; set; }

			[JsonProperty("olympicsParticipation")]
			public bool OlympicsParticipation { get; set; }

			[JsonProperty("conferencesInUse")]
			public bool ConferencesInUse { get; set; }

			[JsonProperty("divisionsInUse")]
			public bool DivisionsInUse { get; set; }

			[JsonProperty("wildCardInUse")]
			public bool WildCardInUse { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this SeasonsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
