using System;
using System.Collections.Generic;
using System.Threading;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;

namespace SvnBridge.Cache
{
	[Singleton]
	public class MemoryBasedPersistentCache : IPersistentCache
	{
		private readonly IDictionary<string, object> cache = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
		private readonly ReaderWriterLock rwLock = new ReaderWriterLock();

		#region IPersistentCache Members

		public CachedResult Get(string key)
		{
			CachedResult result = null;
			ReadLock(delegate
			{
				object val;
				if (cache.TryGetValue(key, out val))
					result = new CachedResult(val);
			});
			return result;
		}

		private void ReadLock(Action action)
		{
			if (rwLock.IsReaderLockHeld || rwLock.IsWriterLockHeld)
			{
				action();
				return;
			}
			rwLock.AcquireReaderLock(Timeout.Infinite);
			try
			{
				action();
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}

		public void Set(string key, object obj)
		{
			Lock(key, delegate
			{
				cache[key] = obj;
			});
		}

		public void Lock(string key, Action action)
		{
			if (rwLock.IsWriterLockHeld)
			{
				action();
				return;
			}

			rwLock.AcquireWriterLock(Timeout.Infinite);
			try
			{
				action();
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}

		public bool Contains(string key)
		{
			bool result = false;
			ReadLock(delegate
			{
				result = cache.ContainsKey(key);
			});
			return result;
		}

		public void Clear()
		{
			rwLock.AcquireWriterLock(Timeout.Infinite);
			try
			{
				cache.Clear();
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}

		public void Add(string key, string value)
		{
			Lock(key, delegate
			{
				ISet<string> items;
				object temp;
				if (cache.TryGetValue(key, out temp))
					items = (ISet<string>)temp;
				else
					cache[key] = items = new HashSet<string>();
				items.Add(value);
			});
		}

		public List<T> GetList<T>(string key)
		{
			List<T> items = new List<T>();
			ReadLock(delegate
			{
				CachedResult result = Get(key);
				if (result == null)
					return;

				if (result.Value is T)
				{
					items.Add((T)result.Value);
					return;
				}

				foreach (string itemKey in (IEnumerable<string>)result.Value)
				{
					CachedResult itemResult = Get(itemKey);
					if (itemResult != null)
						items.Add((T)itemResult.Value);
				}
				return;
			});
			return items;
		}

		#endregion
	}
}