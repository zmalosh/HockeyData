using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Processors.NhlCom.Feeds
{
	public class TeamsFeed
	{
		public static string GetFeedUrl(string nhlSeasonKey)
		{
			return $"https://statsapi.web.nhl.com/api/v1/teams?season={nhlSeasonKey}";
		}

		public static TeamsFeed FromJson(string json) => JsonConvert.DeserializeObject<TeamsFeed>(json, Converter.Settings);

		[JsonProperty("copyright")]
		public string Copyright { get; set; }

		[JsonProperty("teams")]
		public List<ApiTeam> Teams { get; set; }

		public class ApiTeam
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("abbreviation")]
			public string Abbreviation { get; set; }

			[JsonProperty("teamName")]
			public string TeamName { get; set; }

			[JsonProperty("locationName")]
			public string LocationName { get; set; }

			[JsonProperty("firstYearOfPlay")]
			public int? FirstYearOfPlay { get; set; }

			[JsonProperty("division")]
			public ApiDivision Division { get; set; }

			[JsonProperty("conference")]
			public ApiConference Conference { get; set; }

			[JsonProperty("franchise")]
			public ApiFranchise Franchise { get; set; }

			[JsonProperty("shortName")]
			public string ShortName { get; set; }

			[JsonProperty("franchiseId")]
			public int FranchiseId { get; set; }

			[JsonProperty("active")]
			public bool Active { get; set; }

			[JsonProperty("venue", NullValueHandling = NullValueHandling.Ignore)]
			public ApiVenue Venue { get; set; }

			[JsonProperty("officialSiteUrl", NullValueHandling = NullValueHandling.Ignore)]
			public string OfficialSiteUrl { get; set; }
		}

		public class ApiConference
		{
			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
			public int? Id { get; set; }

			[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
			public string Name { get; set; }
		}

		public class ApiDivision
		{
			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
			public int? Id { get; set; }

			[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
			public string Name { get; set; }

			[JsonProperty("nameShort", NullValueHandling = NullValueHandling.Ignore)]
			public string NameShort { get; set; }

			[JsonProperty("abbreviation", NullValueHandling = NullValueHandling.Ignore)]
			public string Abbreviation { get; set; }
		}

		public class ApiFranchise
		{
			[JsonProperty("franchiseId")]
			public int FranchiseId { get; set; }

			[JsonProperty("teamName")]
			public string TeamName { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiVenue
		{
			[JsonProperty("id")]
			public int? Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("city")]
			public string City { get; set; }

			[JsonProperty("timeZone")]
			public ApiTimeZone TimeZone { get; set; }
		}

		public class ApiTimeZone
		{
			[JsonProperty("id")]
			public string TimeZoneName { get; set; }

			[JsonProperty("offset")]
			public int Offset { get; set; }

			[JsonProperty("tz")]
			public string TimeZoneAbbr { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this TeamsFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
