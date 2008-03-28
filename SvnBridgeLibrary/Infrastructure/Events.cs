using System;
using System.Collections.Generic;

namespace SvnBridge.Infrastructure
{
	public static class Events
	{
		public delegate void CachingRevisionAction(string serverUrl, int revision);

		private static readonly List<WeakReference> beginCaching = new List<WeakReference>();
		private static readonly List<WeakReference> endCaching = new List<WeakReference>();

		public static event CachingRevisionAction StartingCachingRevision
		{
			add
			{
				beginCaching.Add(new WeakReference(value));
			}
			remove
			{
				WeakReference item = beginCaching.Find(delegate(WeakReference reference)
				{
					return (CachingRevisionAction) reference.Target == value;
				});
				if (item != null)
					beginCaching.Remove(item);
			}
		}

		public static event CachingRevisionAction FinishedCachingRevision
		{
			add
			{
				endCaching.Add(new WeakReference(value));
			}
			remove
			{
				WeakReference item = endCaching.Find(delegate(WeakReference reference)
				{
					return (CachingRevisionAction)reference.Target == value;
				});
				if (item != null)
					endCaching.Remove(item);
			}
		}

		public static void RaiseStartingCachingRevision(string serverUrl, int revision)
		{
			RaiseEvent(beginCaching.ToArray(), serverUrl, revision);
		}

		public static void RaiseFinishedCachingRevision(string serverUrl, int revision)
		{
			RaiseEvent(endCaching.ToArray(), serverUrl, revision);
		}

		private static void RaiseEvent(IEnumerable<WeakReference> caching, string serverUrl, int revision)
		{
			foreach (WeakReference reference in caching)
			{
				CachingRevisionAction action = (CachingRevisionAction)reference.Target;
				if(action==null)
					continue;
				action(serverUrl, revision);
			}
		}
	}
}