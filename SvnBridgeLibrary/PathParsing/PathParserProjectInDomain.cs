using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.PathParsing
{
	public class PathParserProjectInDomain : PathParserProjectInPath
	{
	    public PathParserProjectInDomain(string server, ProjectInformationRepository projectInformationRepository) : base(server, projectInformationRepository)
	    {
	    }

	    public override string GetProjectName(IHttpRequest request)
		{
			return request.Headers["Host"].Split('.')[0];
		}
	}
}