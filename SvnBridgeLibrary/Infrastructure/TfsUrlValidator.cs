using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Utility;

namespace SvnBridge.Infrastructure
{
	public class TfsUrlValidator : ITfsUrlValidator
	{
		private ICache cache;

		public TfsUrlValidator(ICache cache)
		{
			this.cache = cache;
		}

		public bool IsValidTfsServerUrl(string url)
		{
			string cacheKey = "IsValidTfsServerUrl_" + url;
			CachedResult result = cache.Get(cacheKey);
			if (result != null)
				return (bool) result.Value;
			bool validUrl = Helper.IsValidTFSUrl(url, Proxy.DefaultProxy);
			cache.Set(cacheKey, validUrl);
			return validUrl;
		}
	}
}