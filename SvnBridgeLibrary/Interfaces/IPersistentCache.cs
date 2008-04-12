using System;
using System.Collections.Generic;

namespace SvnBridge.Interfaces
{
	public delegate void Action();

	public interface IPersistentCache
	{
		CachedResult Get(string key);

		void Set(string key, object obj);

		void Lock(string key, Action action);
		
		bool Contains(string key);

		void Clear();

		void Add(string key, string value);

		List<T> GetList<T>(string key);
	}
}