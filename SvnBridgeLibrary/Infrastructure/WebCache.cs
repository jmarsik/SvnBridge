using System.Web;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
    public class WebCache : ICache
    {
        public object Get(string key)
        {
            return HttpContext.Current.Cache[key];
        }

        public void Set(string key, object obj)
        {
            HttpContext.Current.Cache[key] = obj;
        }
    }
}