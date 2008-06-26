namespace SvnBridge.Interfaces
{
	public interface IIgnoredFilesSpecification
	{
		bool ShouldBeIgnored(string file);
	}
}