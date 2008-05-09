using System.Collections;
using System.Collections.Generic;
using SvnBridge.Handlers;

namespace SvnBridge.Infrastructure.Statistics
{
    public interface IActionTracking
    {
        void Request(HttpContextHandlerBase handler);
        void Error();
        IDictionary<string, long> GetStatistics();
    }
}