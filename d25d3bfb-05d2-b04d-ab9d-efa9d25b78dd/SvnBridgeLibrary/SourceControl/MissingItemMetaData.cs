namespace SvnBridge.SourceControl
{
	/// <summary>
	/// This class marks a missing item, usually it occurs when a file has moved through several changes
	/// in a calculated diff.
	/// This is used for both item and folders, because there is no good way to differenciate between the two
	/// when the item is missing, and a folder is-a file.
	/// </summary>
	public class MissingFolderMetaData : FolderMetaData
	{
		public MissingFolderMetaData(string name, int revision)
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