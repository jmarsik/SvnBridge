using SvnBridge.Interfaces;
using SvnBridge.Cache;

namespace SvnBridge.NullImpl
{
	public class NullCache : WebCache
	{
		public override CachedResult Get(string key)
		{
			return null;
		}

		public override void Set(string key, object obj)
		{
		}

	    public override void Clear()
	    {
	    }
	}
}
