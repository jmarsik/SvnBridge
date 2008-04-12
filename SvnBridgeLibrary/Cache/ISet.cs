using System.Collections.Generic;

namespace SvnBridge.Cache
{
	public interface ISet<T> : IEnumerable<T>
	{
		void Add(T item);
	}
}