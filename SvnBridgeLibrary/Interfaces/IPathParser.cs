using SvnBridge.Net;

namespace SvnBridge.Interfaces
{
	public interface IPathParser
	{
		string GetServerUrl(IHttpRequest request);
		string GetLocalPath(IHttpRequest request);
		string GetProjectName(IHttpRequest request);
	}
}
