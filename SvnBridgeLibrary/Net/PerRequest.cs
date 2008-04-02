using System;
using System.Collections;
using System.Web;

namespace SvnBridge.Net
{
	public static class PerRequest
	{
		[ThreadStatic] private static IDictionary currentItems;

		public static void Init()
		{
			currentItems = new Hashtable();
		}

		public static IDictionary Items
		{
			get
			{
				if (HttpContext.Current != null)
					return HttpContext.Current.Items;
				return currentItems;
			}
		}
	}
}