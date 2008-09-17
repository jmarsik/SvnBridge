using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using System.Net;
using System;

namespace SvnBridge.PathParsing
{
	public class PathParserProjectInDomain : PathParserSingleServerWithProjectInPath
	{
	    public PathParserProjectInDomain(string server, ProjectInformationRepository projectInformationRepository)
	    {
            foreach (string singleServerUrl in server.Split(','))
            {
                Uri ignored;
                if (Uri.TryCreate(singleServerUrl, UriKind.Absolute, out ignored) == false)
                    throw new InvalidOperationException("The url '" + server + "' is not a valid url");
            }
            this.server = server;
            this.projectInformationRepository = projectInformationRepository;
        }

        public override string GetServerUrl(IHttpRequest request, ICredentials credentials)
        {
            string projectName = GetProjectName(request);
            return projectInformationRepository.GetProjectLocation(credentials, projectName).ServerUrl;
        }

	    public override string GetProjectName(IHttpRequest request)
		{
			return request.Headers["Host"].Split('.')[0];
		}
	}
}