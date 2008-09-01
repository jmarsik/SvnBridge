using System.IO;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
	public class OldSvnBridgeFilesSpecification
	{
		public bool ShouldBeIgnored(string file)
		{
			return Path.GetFileName(file) == "..svnbridge";
		}
	}
}