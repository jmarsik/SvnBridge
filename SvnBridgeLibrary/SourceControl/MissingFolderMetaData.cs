namespace SvnBridge.SourceControl
{
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