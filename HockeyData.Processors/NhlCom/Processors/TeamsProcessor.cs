using HockeyData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyData.Processors.NhlCom.Processors
{
	public class TeamsProcessor : IProcessor
	{
		private readonly string nhlSeasonKey;

		public TeamsProcessor(string nhlSeasonKey = null)
		{
			this.nhlSeasonKey = nhlSeasonKey;
		}

		public void Run(HockeyDataContext dbContext)
		{
			var url = Feeds.TeamsFeed.GetFeedUrl(this.nhlSeasonKey);
			var rawJson = JsonUtility.GetRawJsonFromUrl(url);
			var feed = Feeds.TeamsFeed.FromJson(rawJson);

			var teamsDict = dbContext.Teams.ToDictionary(x => x.NhlTeamId, y => y);
			var apiTeams = feed.Teams.OrderBy(x => x.Name).ToList();

			foreach (var apiTeam in apiTeams)
			{
				if (!teamsDict.TryGetValue(apiTeam.Id, out Team dbTeam))
				{
					dbTeam = new Team
					{
						NhlTeamId = apiTeam.Id,
						FirstYearOfPlay = apiTeam.FirstYearOfPlay,
						TeamAbbr = apiTeam.Abbreviation,
						TeamFullName = apiTeam.Name,
						TeamLocation = apiTeam.LocationName,
						TeamName = apiTeam.TeamName,
						TeamShortName = apiTeam.ShortName,
						WebSiteUrl = apiTeam.OfficialSiteUrl
					};
					teamsDict.Add(apiTeam.Id, dbTeam);
					dbContext.Teams.Add(dbTeam);
					dbContext.SaveChanges();
				}
				else if (HasUpdates(dbTeam, apiTeam))
				{
					dbTeam.TeamFullName = apiTeam.Name;
					dbTeam.TeamLocation = apiTeam.LocationName;
					dbTeam.TeamName = apiTeam.TeamName;
					dbTeam.TeamShortName = apiTeam.ShortName;
					dbTeam.WebSiteUrl = apiTeam.OfficialSiteUrl;
					dbTeam.TeamAbbr = apiTeam.Abbreviation;
					dbContext.SaveChanges();
				}
			}
		}

		private bool HasUpdates(Team dbTeam, Feeds.TeamsFeed.ApiTeam apiTeam)
		{
			return dbTeam.TeamFullName != apiTeam.Name
				|| dbTeam.TeamLocation != apiTeam.LocationName
				|| dbTeam.TeamName != apiTeam.TeamName
				|| dbTeam.TeamShortName != apiTeam.ShortName
				|| dbTeam.WebSiteUrl != apiTeam.OfficialSiteUrl
				|| dbTeam.TeamAbbr != apiTeam.Abbreviation;
		}
	}
}
