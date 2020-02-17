using HockeyData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyData.Processors.NhlCom.Processors
{
	public class SeasonsProcessor : IProcessor
	{
		private readonly JsonUtility JsonUtility;
		
		public SeasonsProcessor(string nhlSeasonKey = null)
		{
			this.JsonUtility = new JsonUtility(7 * 24 * 60 * 60);
		}

		public void Run(HockeyDataContext dbContext)
		{
			var url = Feeds.SeasonsFeed.GetFeedUrl();
			var rawJson = this.JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.SeasonsFeed.FromJson(rawJson);

			int nhlLeagueId = dbContext.Leagues.Single(x => x.LeagueAbbr == "NHL").LeagueId;
			var seasonsDict = dbContext.Seasons.ToDictionary(x => x.NhlSeasonKey, y => y);
			var apiSeasons = feed.Seasons.OrderBy(x => x.SeasonId).ToList();

			foreach (var apiSeason in apiSeasons)
			{
				if (!seasonsDict.ContainsKey(apiSeason.SeasonId))
				{
					var dbSeason = new Season
					{
						LeagueId = nhlLeagueId,
						NhlSeasonKey = apiSeason.SeasonId,
						HasConferences = apiSeason.ConferencesInUse,
						HasDivisions = apiSeason.DivisionsInUse,
						HasOlympics = apiSeason.OlympicsParticipation,
						HasTies = apiSeason.TiesInUse,
						HasWildCard = apiSeason.WildCardInUse
					};
					seasonsDict.Add(dbSeason.NhlSeasonKey, dbSeason);
					dbContext.Seasons.Add(dbSeason);
					dbContext.SaveChanges();
				}
			}
		}
	}
}
