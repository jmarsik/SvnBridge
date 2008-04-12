using System;
using System.Collections;
using System.Collections.Generic;

namespace SvnBridge.Cache
{
	[Serializable]
	public class HashSet<T> : ISet<T>
	{
		private Dictionary<T, object> inner = new Dictionary<T, object>();

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>) this).GetEnumerator();
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return inner.Keys.GetEnumerator();
		}

		#endregion

		#region ISet<T> Members

		public void Add(T item)
		{
			inner[item] = null;
		}

		#endregion
	}
}