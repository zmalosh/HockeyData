using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Processors.NhlCom.Feeds
{
	public class GameLiveFeed
	{
		public static string GetFeedUrl(string nhlGameId = null)
		{
			return $"https://statsapi.web.nhl.com/api/v1/game/{nhlGameId}";
		}

		public static GameLiveFeed FromJson(string json) => JsonConvert.DeserializeObject<GameLiveFeed>(json, Converter.Settings);

		[JsonProperty("copyright")]
		public string Copyright { get; set; }

		[JsonProperty("gamePk")]
		public long GamePk { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("metaData")]
		public ApiMetaData MetaData { get; set; }

		[JsonProperty("gameData")]
		public ApiGameData GameData { get; set; }

		[JsonProperty("liveData")]
		public ApiLiveData LiveData { get; set; }


		public class ApiGameData
		{
			[JsonProperty("game")]
			public ApiGame Game { get; set; }

			[JsonProperty("datetime")]
			public ApiGameTimes Datetime { get; set; }

			[JsonProperty("status")]
			public ApiStatus Status { get; set; }

			[JsonProperty("teams")]
			public ApiTeams Teams { get; set; }

			[JsonProperty("players")]
			public Dictionary<string, ApiPlayerDetailed> Players { get; set; }

			[JsonProperty("venue")]
			public ApiSimpleEntity Venue { get; set; }
		}

		public class ApiGameTimes
		{
			[JsonProperty("dateTime")]
			public DateTimeOffset StartDateTime { get; set; }

			[JsonProperty("endDateTime")]
			public DateTimeOffset EndDateTime { get; set; }
		}

		public class ApiGame
		{
			[JsonProperty("pk")]
			public int GameId { get; set; }

			[JsonProperty("season")]
			public string SeasonKey { get; set; }

			[JsonProperty("type")]
			public string GameType { get; set; }
		}

		public class ApiPlayerDetailed
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("fullName")]
			public string FullName { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("firstName")]
			public string FirstName { get; set; }

			[JsonProperty("lastName")]
			public string LastName { get; set; }

			[JsonProperty("primaryNumber")]
			public int PrimaryNumber { get; set; }

			[JsonProperty("birthDate")]
			public DateTimeOffset BirthDate { get; set; }

			[JsonProperty("currentAge")]
			public int CurrentAge { get; set; }

			[JsonProperty("birthCity")]
			public string BirthCity { get; set; }

			[JsonProperty("birthCountry")]
			public string BirthCountry { get; set; }

			[JsonProperty("nationality")]
			public string Nationality { get; set; }

			[JsonProperty("height")]
			public string Height { get; set; }

			[JsonProperty("weight")]
			public int Weight { get; set; }

			[JsonProperty("active")]
			public bool Active { get; set; }

			[JsonProperty("alternateCaptain")]
			public bool AlternateCaptain { get; set; }

			[JsonProperty("captain")]
			public bool Captain { get; set; }

			[JsonProperty("rookie")]
			public bool Rookie { get; set; }

			[JsonProperty("shootsCatches")]
			public string ShootsCatches { get; set; }

			[JsonProperty("rosterStatus")]
			public string RosterStatus { get; set; }

			[JsonProperty("currentTeam")]
			public ApiSimpleEntity CurrentTeam { get; set; }

			[JsonProperty("primaryPosition")]
			public ApiPosition PrimaryPosition { get; set; }

			[JsonProperty("birthStateProvince", NullValueHandling = NullValueHandling.Ignore)]
			public string BirthStateProvince { get; set; }
		}

		public class ApiSimpleEntity
		{
			[JsonProperty("id")]
			public long Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("triCode", NullValueHandling = NullValueHandling.Ignore)]
			public string TriCode { get; set; }

			[JsonProperty("abbreviation", NullValueHandling = NullValueHandling.Ignore)]
			public string Abbreviation { get; set; }
		}

		public class ApiPosition
		{
			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("abbreviation")]
			public string Abbreviation { get; set; }
		}

		public class ApiStatus
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
			public ApiTeamData Away { get; set; }

			[JsonProperty("home")]
			public ApiTeamData Home { get; set; }
		}

		public class ApiTeamData
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("venue")]
			public ApiVenue Venue { get; set; }

			[JsonProperty("abbreviation")]
			public string Abbreviation { get; set; }

			[JsonProperty("triCode")]
			public string TriCode { get; set; }

			[JsonProperty("teamName")]
			public string TeamName { get; set; }

			[JsonProperty("locationName")]
			public string LocationName { get; set; }

			[JsonProperty("firstYearOfPlay")]
			public int FirstYearOfPlay { get; set; }

			[JsonProperty("division")]
			public ApiDivision Division { get; set; }

			[JsonProperty("conference")]
			public ApiSimpleEntity Conference { get; set; }

			[JsonProperty("franchise")]
			public ApiFranchise Franchise { get; set; }

			[JsonProperty("shortName")]
			public string ShortName { get; set; }

			[JsonProperty("officialSiteUrl")]
			public string OfficialSiteUrl { get; set; }

			[JsonProperty("franchiseId")]
			public int FranchiseId { get; set; }

			[JsonProperty("active")]
			public bool Active { get; set; }
		}

		public class ApiDivision
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("nameShort")]
			public string NameShort { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("abbreviation")]
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
			public int Id { get; set; }

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
			public string Id { get; set; }

			[JsonProperty("offset")]
			public int Offset { get; set; }

			[JsonProperty("tz")]
			public string Tz { get; set; }
		}

		public class ApiLiveData
		{
			[JsonProperty("plays")]
			public ApiPlays Plays { get; set; }

			[JsonProperty("linescore")]
			public ApiLinescore Linescore { get; set; }

			[JsonProperty("boxscore")]
			public ApiBoxscore Boxscore { get; set; }

			[JsonProperty("decisions")]
			public ApiGameDecisions Decisions { get; set; }
		}

		public class ApiBoxscore
		{
			[JsonProperty("teams")]
			public ApiBoxscoreTeams Teams { get; set; }

			[JsonProperty("officials")]
			public List<ApiOfficial> Officials { get; set; }
		}

		public class ApiOfficial
		{
			[JsonProperty("official")]
			public ApiSimplePerson Official { get; set; }

			[JsonProperty("officialType")]
			public string OfficialType { get; set; }
		}

		public class ApiSimplePerson
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("fullName")]
			public string FullName { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiBoxscoreTeams
		{
			[JsonProperty("away")]
			public ApiBoxscoreTeam Away { get; set; }

			[JsonProperty("home")]
			public ApiBoxscoreTeam Home { get; set; }
		}

		public class ApiBoxscoreTeam
		{
			[JsonProperty("team")]
			public ApiSimpleEntity Team { get; set; }

			[JsonProperty("teamStats")]
			public ApiBoxscoreTeamStats TeamStats { get; set; }

			[JsonProperty("players")]
			public ApiBoxscorePlayers Players { get; set; }

			[JsonProperty("goalies")]
			public List<int> Goalies { get; set; }

			[JsonProperty("skaters")]
			public List<int> Skaters { get; set; }

			[JsonProperty("onIce")]
			public List<int> OnIce { get; set; }

			[JsonProperty("onIcePlus")]
			public List<ApiOnIcePlus> OnIcePlus { get; set; }

			[JsonProperty("scratches")]
			public List<int> Scratches { get; set; }

			[JsonProperty("penaltyBox")]
			public List<object> PenaltyBox { get; set; }

			[JsonProperty("coaches")]
			public List<ApiCoach> Coaches { get; set; }
		}

		public class ApiCoach
		{
			[JsonProperty("person")]
			public ApiCoachPerson Person { get; set; }

			[JsonProperty("position")]
			public ApiPosition Position { get; set; }
		}

		public class ApiCoachPerson
		{
			[JsonProperty("fullName")]
			public string FullName { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }
		}

		public class ApiOnIcePlus
		{
			[JsonProperty("playerId")]
			public int PlayerId { get; set; }

			[JsonProperty("shiftDuration")]
			public int ShiftDuration { get; set; }

			[JsonProperty("stamina")]
			public int Stamina { get; set; }
		}

		public class ApiBoxscorePlayers
		{
			[JsonExtensionData]
			public Dictionary<string, ApiBoxscorePlayer> Players { get; set; }
		}

		public class ApiBoxscorePlayer
		{
			[JsonProperty("person")]
			public ApiSimplePlayer Person { get; set; }

			[JsonProperty("jerseyNumber")]
			public int JerseyNumber { get; set; }

			[JsonProperty("position")]
			public ApiPosition Position { get; set; }

			[JsonProperty("stats")]
			public ApiBoxscorePlayerStats Stats { get; set; }
		}

		public class ApiSimplePlayer
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("fullName")]
			public string FullName { get; set; }

			[JsonProperty("link")]
			public string Link { get; set; }

			[JsonProperty("shootsCatches")]
			public string ShootsCatches { get; set; }

			[JsonProperty("rosterStatus")]
			public string RosterStatus { get; set; }
		}

		public class ApiBoxscorePlayerStats
		{
			[JsonProperty("skaterStats")]
			public ApiSkaterStats SkaterStats { get; set; }

			[JsonProperty("goalieStats")]
			public ApiGoalieStats GoalieStats { get; set; }
		}

		public class ApiSkaterStats
		{
			[JsonProperty("timeOnIce")]
			public string TimeOnIce { get; set; }

			[JsonProperty("assists")]
			public int? Assists { get; set; }

			[JsonProperty("goals")]
			public int? Goals { get; set; }

			[JsonProperty("shots")]
			public int? Shots { get; set; }

			[JsonProperty("hits")]
			public int? Hits { get; set; }

			[JsonProperty("powerPlayGoals")]
			public int? PowerPlayGoals { get; set; }

			[JsonProperty("powerPlayAssists")]
			public int? PowerPlayAssists { get; set; }

			[JsonProperty("penaltyMinutes")]
			public int? PenaltyMinutes { get; set; }

			[JsonProperty("faceOffPct", NullValueHandling = NullValueHandling.Ignore)]
			public double? FaceOffPct { get; set; }

			[JsonProperty("faceOffWins")]
			public int? FaceOffWins { get; set; }

			[JsonProperty("faceoffTaken")]
			public int? FaceoffTaken { get; set; }

			[JsonProperty("takeaways")]
			public int? Takeaways { get; set; }

			[JsonProperty("giveaways")]
			public int? Giveaways { get; set; }

			[JsonProperty("shortHandedGoals")]
			public int? ShortHandedGoals { get; set; }

			[JsonProperty("shortHandedAssists")]
			public int? ShortHandedAssists { get; set; }

			[JsonProperty("blocked")]
			public int? Blocked { get; set; }

			[JsonProperty("plusMinus")]
			public int? PlusMinus { get; set; }

			[JsonProperty("evenTimeOnIce")]
			public string EvenTimeOnIce { get; set; }

			[JsonProperty("powerPlayTimeOnIce")]
			public string PowerPlayTimeOnIce { get; set; }

			[JsonProperty("shortHandedTimeOnIce")]
			public string ShortHandedTimeOnIce { get; set; }
		}

		public class ApiGoalieStats
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

			[JsonProperty("saves")]
			public int? Saves { get; set; }

			[JsonProperty("powerPlaySaves")]
			public int? PowerPlaySaves { get; set; }

			[JsonProperty("shortHandedSaves")]
			public int? ShortHandedSaves { get; set; }

			[JsonProperty("evenSaves")]
			public int? EvenSaves { get; set; }

			[JsonProperty("shortHandedShotsAgainst")]
			public int? ShortHandedShotsAgainst { get; set; }

			[JsonProperty("evenShotsAgainst")]
			public int? EvenShotsAgainst { get; set; }

			[JsonProperty("powerPlayShotsAgainst")]
			public int? PowerPlayShotsAgainst { get; set; }

			[JsonProperty("decision")]
			public string Decision { get; set; }

			[JsonProperty("savePercentage")]
			public double? SavePercentage { get; set; }

			[JsonProperty("powerPlaySavePercentage", NullValueHandling = NullValueHandling.Ignore)]
			public double? PowerPlaySavePercentage { get; set; }

			[JsonProperty("evenStrengthSavePercentage")]
			public double? EvenStrengthSavePercentage { get; set; }
		}

		public class ApiBoxscoreTeamStats
		{
			[JsonProperty("teamSkaterStats")]
			public ApiTeamStats TeamSkaterStats { get; set; }
		}

		public class ApiTeamStats
		{
			[JsonProperty("goals")]
			public int? Goals { get; set; }

			[JsonProperty("pim")]
			public int? Pim { get; set; }

			[JsonProperty("shots")]
			public int? Shots { get; set; }

			[JsonProperty("powerPlayPercentage")]
			public string PowerPlayPercentage { get; set; }

			[JsonProperty("powerPlayGoals")]
			public int? PowerPlayGoals { get; set; }

			[JsonProperty("powerPlayOpportunities")]
			public int? PowerPlayOpportunities { get; set; }

			[JsonProperty("faceOffWinPercentage")]
			public string FaceOffWinPercentage { get; set; }

			[JsonProperty("blocked")]
			public int? Blocked { get; set; }

			[JsonProperty("takeaways")]
			public int? Takeaways { get; set; }

			[JsonProperty("giveaways")]
			public int? Giveaways { get; set; }

			[JsonProperty("hits")]
			public int? Hits { get; set; }
		}

		public class ApiGameDecisions
		{
			[JsonProperty("winner")]
			public ApiSimplePerson Winner { get; set; }

			[JsonProperty("loser")]
			public ApiSimplePerson Loser { get; set; }

			[JsonProperty("firstStar")]
			public ApiSimplePerson FirstStar { get; set; }

			[JsonProperty("secondStar")]
			public ApiSimplePerson SecondStar { get; set; }

			[JsonProperty("thirdStar")]
			public ApiSimplePerson ThirdStar { get; set; }
		}

		public class ApiLinescore
		{
			[JsonProperty("currentPeriod")]
			public int CurrentPeriod { get; set; }

			[JsonProperty("currentPeriodOrdinal")]
			public string CurrentPeriodOrdinal { get; set; }

			[JsonProperty("currentPeriodTimeRemaining")]
			public string CurrentPeriodTimeRemaining { get; set; }

			[JsonProperty("periods")]
			public List<ApiLinescorePeriod> Periods { get; set; }

			[JsonProperty("shootoutInfo")]
			public ApiShootout ShootoutInfo { get; set; }

			[JsonProperty("teams")]
			public ApiLinescoreTeams Teams { get; set; }

			[JsonProperty("powerPlayStrength")]
			public string PowerPlayStrength { get; set; }

			[JsonProperty("hasShootout")]
			public bool HasShootout { get; set; }

			[JsonProperty("intermissionInfo")]
			public ApiIntermissionInfo IntermissionInfo { get; set; }

			[JsonProperty("powerPlayInfo")]
			public ApiPowerPlay PowerPlayInfo { get; set; }
		}

		public class ApiIntermissionInfo
		{
			[JsonProperty("intermissionTimeRemaining")]
			public int IntermissionTimeRemaining { get; set; }

			[JsonProperty("intermissionTimeElapsed")]
			public int IntermissionTimeElapsed { get; set; }

			[JsonProperty("inIntermission")]
			public bool InIntermission { get; set; }
		}

		public class ApiLinescorePeriod
		{
			[JsonProperty("periodType")]
			public string PeriodType { get; set; }

			[JsonProperty("startTime")]
			public DateTimeOffset StartTime { get; set; }

			[JsonProperty("endTime")]
			public DateTimeOffset EndTime { get; set; }

			[JsonProperty("num")]
			public int Num { get; set; }

			[JsonProperty("ordinalNum")]
			public string OrdinalNum { get; set; }

			[JsonProperty("home")]
			public ApiTeamPeriod Home { get; set; }

			[JsonProperty("away")]
			public ApiTeamPeriod Away { get; set; }
		}

		public class ApiTeamPeriod
		{
			[JsonProperty("goals")]
			public int Goals { get; set; }

			[JsonProperty("shotsOnGoal")]
			public int ShotsOnGoal { get; set; }

			[JsonProperty("rinkSide")]
			public string RinkSide { get; set; }
		}

		public class ApiPowerPlay
		{
			[JsonProperty("situationTimeRemaining")]
			public int SituationTimeRemaining { get; set; }

			[JsonProperty("situationTimeElapsed")]
			public int SituationTimeElapsed { get; set; }

			[JsonProperty("inSituation")]
			public bool InSituation { get; set; }
		}

		public class ApiShootout
		{
			[JsonProperty("away")]
			public ApiTeamShootout Away { get; set; }

			[JsonProperty("home")]
			public ApiTeamShootout Home { get; set; }
		}

		public class ApiTeamShootout
		{
			[JsonProperty("scores")]
			public int Scores { get; set; }

			[JsonProperty("attempts")]
			public int Attempts { get; set; }
		}

		public class ApiLinescoreTeams
		{
			[JsonProperty("home")]
			public ApiTeamLinescore Home { get; set; }

			[JsonProperty("away")]
			public ApiTeamLinescore Away { get; set; }
		}

		public class ApiTeamLinescore
		{
			[JsonProperty("team")]
			public ApiSimpleEntity Team { get; set; }

			[JsonProperty("goals")]
			public int Goals { get; set; }

			[JsonProperty("shotsOnGoal")]
			public int ShotsOnGoal { get; set; }

			[JsonProperty("goaliePulled")]
			public bool GoaliePulled { get; set; }

			[JsonProperty("numSkaters")]
			public int NumSkaters { get; set; }

			[JsonProperty("powerPlay")]
			public bool PowerPlay { get; set; }
		}

		public class ApiPlays
		{
			[JsonProperty("allPlays")]
			public List<ApiPlay> AllPlays { get; set; }

			[JsonProperty("scoringPlays")]
			public List<int> ScoringPlays { get; set; }

			[JsonProperty("penaltyPlays")]
			public List<int> PenaltyPlays { get; set; }

			[JsonProperty("playsByPeriod")]
			public List<ApiPlaysByPeriod> PlaysByPeriod { get; set; }

			[JsonProperty("currentPlay")]
			public ApiCurrentPlay CurrentPlay { get; set; }
		}

		public class ApiPlay
		{
			[JsonProperty("result")]
			public ApiPlayResult Result { get; set; }

			[JsonProperty("about")]
			public ApiPlayInfo About { get; set; }

			[JsonProperty("coordinates")]
			public ApiPlayCoordinates Coordinates { get; set; }

			[JsonProperty("players", NullValueHandling = NullValueHandling.Ignore)]
			public List<ApiPlayPlayer> Players { get; set; }

			[JsonProperty("team", NullValueHandling = NullValueHandling.Ignore)]
			public ApiSimpleEntity Team { get; set; }
		}

		public class ApiPlayInfo
		{
			[JsonProperty("eventIdx")]
			public int EventIdx { get; set; }

			[JsonProperty("eventId")]
			public int EventId { get; set; }

			[JsonProperty("period")]
			public int Period { get; set; }

			[JsonProperty("periodType")]
			public string PeriodType { get; set; }

			[JsonProperty("ordinalNum")]
			public string OrdinalNum { get; set; }

			[JsonProperty("periodTime")]
			public string PeriodTime { get; set; }

			[JsonProperty("periodTimeRemaining")]
			public string PeriodTimeRemaining { get; set; }

			[JsonProperty("dateTime")]
			public DateTimeOffset DateTime { get; set; }

			[JsonProperty("goals")]
			public ApiGoalStatus Goals { get; set; }
		}

		public class ApiGoalStatus
		{
			[JsonProperty("away")]
			public int Away { get; set; }

			[JsonProperty("home")]
			public int Home { get; set; }
		}

		public class ApiPlayCoordinates
		{
			[JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
			public int? X { get; set; }

			[JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
			public int? Y { get; set; }
		}

		public class ApiPlayPlayer
		{
			[JsonProperty("player")]
			public ApiSimplePerson Player { get; set; }

			[JsonProperty("playerType")]
			public string PlayerType { get; set; }

			[JsonProperty("seasonTotal", NullValueHandling = NullValueHandling.Ignore)]
			public int? SeasonTotal { get; set; }
		}

		public class ApiPlayResult
		{
			[JsonProperty("event")]
			public string Event { get; set; }

			[JsonProperty("eventCode")]
			public string EventCode { get; set; }

			[JsonProperty("eventTypeId")]
			public string EventTypeId { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("secondaryType", NullValueHandling = NullValueHandling.Ignore)]
			public string SecondaryType { get; set; }

			[JsonProperty("strength", NullValueHandling = NullValueHandling.Ignore)]
			public ApiPlayStrength Strength { get; set; }

			[JsonProperty("gameWinningGoal", NullValueHandling = NullValueHandling.Ignore)]
			public bool? GameWinningGoal { get; set; }

			[JsonProperty("emptyNet", NullValueHandling = NullValueHandling.Ignore)]
			public bool? EmptyNet { get; set; }

			[JsonProperty("penaltySeverity", NullValueHandling = NullValueHandling.Ignore)]
			public string PenaltySeverity { get; set; }

			[JsonProperty("penaltyMinutes", NullValueHandling = NullValueHandling.Ignore)]
			public int? PenaltyMinutes { get; set; }
		}

		public class ApiPlayStrength
		{
			[JsonProperty("code")]
			public string Code { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }
		}

		public class ApiCurrentPlay
		{
			[JsonProperty("result")]
			public ApiCurrentPlayResult Result { get; set; }

			[JsonProperty("about")]
			public ApiPlayInfo About { get; set; }

			[JsonProperty("coordinates")]
			public ApiPlayCoordinates Coordinates { get; set; }
		}

		public class ApiCurrentPlayResult
		{
			[JsonProperty("event")]
			public string Event { get; set; }

			[JsonProperty("eventCode")]
			public string EventCode { get; set; }

			[JsonProperty("eventTypeId")]
			public string EventTypeId { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }
		}

		public class ApiPlaysByPeriod
		{
			[JsonProperty("startIndex")]
			public int StartIndex { get; set; }

			[JsonProperty("plays")]
			public List<int> Plays { get; set; }

			[JsonProperty("endIndex")]
			public int EndIndex { get; set; }
		}

		public class ApiMetaData
		{
			[JsonProperty("wait")]
			public int Wait { get; set; }

			[JsonProperty("timeStamp")]
			public string TimeStamp { get; set; }
		}
	}

	public static partial class Serialize
	{
		public static string ToJson(this GameLiveFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
