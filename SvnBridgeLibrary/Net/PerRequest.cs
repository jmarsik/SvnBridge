using System;
using System.Collections;
using System.Web;

namespace SvnBridge.Net
{
	public static class PerRequest
	{
		[ThreadStatic] private static IDictionary currentItems;
	    private static bool? runningOnWeb;

	    public static void Init()
		{
			currentItems = new Hashtable();
		}

        public static void Dispose()
        {
            currentItems = null;
        }

		public static IDictionary Items
		{
			get
			{
				EnsureInitialized();
			    if (RunningOnWeb)
					return HttpContext.Current.Items;
				return currentItems;
			}
		}

	    private static bool RunningOnWeb
	    {
	        get
	        {
                if (runningOnWeb == null)
                    runningOnWeb = HttpContext.Current != null;
	            return runningOnWeb.Value;
	        }
	    }

		public static bool IsInitialized
		{
			get
			{
				return RunningOnWeb || currentItems != null;
			}
		}

		public static void EnsureInitialized()
		{
            if (RunningOnWeb == false && currentItems == null)
				throw new InvalidOperationException("Cannot use PerRequest Items if it wasn't initialized");
		}
	}
}
