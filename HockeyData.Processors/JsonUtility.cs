using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HockeyData.Processors
{
	public static class JsonUtility
	{
		private static readonly WebClient WebClient = CreateWebClient();

		public static string GetRawJsonFromUrl(string url)
		{
			var rawJson = WebClient.DownloadString(url);
			return rawJson;
		}

		private static WebClient CreateWebClient()
		{
			var client = new WebClient();
			return client;
		}
	}
}