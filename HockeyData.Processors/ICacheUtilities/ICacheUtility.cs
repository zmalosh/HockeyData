using System;
using System.Collections.Generic;
using System.Text;

namespace HockeyData.Processors.ICacheUtilities
{
	public interface ICacheUtility
	{
		bool ReadFile(string path, out string text, int? cacheTimeSeconds = null);
		void WriteFile(string path, string text);
	}
}
