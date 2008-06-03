using System.Collections.Generic;
using SvnBridge.Handlers;
using SvnBridge.Infrastructure.Statistics;

namespace SvnBridge.NullImpl
{
	public class NullActionTracking : IActionTracking
	{
		public void Request(HttpContextHandlerBase handler)
		{
		}

		public void Error()
		{
			
		}

		public IDictionary<string, long> GetStatistics()
		{
			Dictionary<string, long> statistics = new Dictionary<string, long>();
			statistics["no statistics are gathered"] = 0L;
			return statistics;
		}
	}
}