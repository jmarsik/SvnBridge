namespace SvnBridge
{
	using System.IO;

	public static class Constants
	{
		public const int BufferSize = 1024*32;
		public const int MaxPort = 65535;
		public const string ServerRootPath = "$/";
		public const string SvnVccPath = "/!svn/vcc/default";
		public const string FolderPropFile = ".svnbridge";
		public static readonly string LocalPrefix = Path.GetTempFileName();
		public const string PropFolder = "..svnbridge";
	}
}