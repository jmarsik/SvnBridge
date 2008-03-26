namespace SvnBridge.Interfaces
{
	public interface ITfsUrlValidator
	{
		bool IsValidTfsServerUrl(string url);
	}
}