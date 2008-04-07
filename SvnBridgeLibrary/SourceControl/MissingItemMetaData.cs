namespace SvnBridge.SourceControl
{
	/// <summary>
	/// This class marks a missing item, usually it occurs when a file has moved through several changes
	/// in a calculated diff
	/// </summary>
	public class MissingItemMetaData : ItemMetaData
	{
		public MissingItemMetaData(string name, int revision)
		{
			Name = name;
			ItemRevision = revision;
		}

		public override string ToString()
		{
			return "Missing: " + base.ToString();
		}
	}
}