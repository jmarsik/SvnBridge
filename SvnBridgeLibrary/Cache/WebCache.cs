using System;
using System.Web;
using System.Web.Caching;
using SvnBridge.Interfaces;

namespace SvnBridge.Cache
{
    public class WebCache : ICache
    {
        private readonly System.Web.Caching.Cache cache = HttpRuntime.Cache;

        public CachedResult Get(string key)
        {
            return (CachedResult)cache[key];
        }

        public void Set(string key, object obj)
        {
            cache.Add(key, 
                new CachedResult(obj), 
                null, 
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                TimeSpan.FromHours(2), 
                CacheItemPriority.Default, 
                null);
        }
    }
}