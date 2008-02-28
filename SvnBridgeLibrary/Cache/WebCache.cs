using System.Web;
using SvnBridge.Interfaces;

namespace SvnBridge.Cache
{
    public class WebCache : ICache
    {
        readonly System.Web.Caching.Cache cache = HttpRuntime.Cache;

        public object Get(string key)
        {
            return cache[key];
        }

        public void Set(string key, object obj)
        {
            cache[key] = obj;
        }
    }
}