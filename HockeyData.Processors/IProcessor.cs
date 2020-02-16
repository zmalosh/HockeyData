using System;

namespace HockeyData.Processors
{
	public interface IProcessor
	{
		void Run(Model.HockeyDataContext dbContext);
	}
}
