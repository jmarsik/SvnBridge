using System;
using System.Runtime.InteropServices;

namespace SvnBridge.Infrastructure
{
	public class SequentialGuid
	{
		public static Guid Next()
		{
			Guid guid;
			UuidCreateSequential(out guid);
			return guid;
		}

		[DllImport("rpcrt4.dll", SetLastError = true)]
		static extern int UuidCreateSequential(out Guid guid);
	}
}