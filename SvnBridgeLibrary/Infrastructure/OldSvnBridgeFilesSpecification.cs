using System.IO;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
	public class OldSvnBridgeFilesSpecification : IIgnoredFilesSpecification
	{
		public bool ShouldBeIgnored(string file)
		{
			return Path.GetExtension(file) == ".svnbridge";
		}
	}
}