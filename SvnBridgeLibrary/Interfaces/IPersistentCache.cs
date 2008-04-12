using System;
using System.Collections.Generic;

namespace SvnBridge.Interfaces
{
	public interface IPersistentCache
	{
		CachedResult Get(string key);

		void Set(string key, object obj);

		void Lock(string key, Action<string> action);
		
		bool Contains(string key);

		void Clear();

		void Add(string key, string value);

		List<T> GetList<T>(string key);
	}
}