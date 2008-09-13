using System.Reflection;
using System.Web;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.PathParsing;
using SvnBridge.SourceControl;
using SvnBridge;

namespace SvnBridgeServer
{
	public class SvnBridgeHttpHandler : IHttpHandler
	{
		private readonly HttpContextDispatcher dispatcher;

        static SvnBridgeHttpHandler()
        {
            new BootStrapper().Start();
        }

		public SvnBridgeHttpHandler()
		{
			string tfsUrl = Configuration.TfsUrl;
		    RequestCache.Items["serverUrl"] = tfsUrl;
		    ProjectInformationRepository projectInformationRepository = Container.Resolve<ProjectInformationRepository>();
            IPathParser pathParser;
		    if (Configuration.UrlIncludesProjectName)
			{
                pathParser = new PathParserProjectInDomain(tfsUrl, projectInformationRepository);
			}
			else
			{
                pathParser = new PathParserProjectInPath(tfsUrl, projectInformationRepository);
			}
            dispatcher = new HttpContextDispatcher(pathParser, Container.Resolve<ActionTrackingViaPerfCounter>());
        }

		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			try
			{
				dispatcher.Dispatch(new HttpContextWrapper(context));
			}
			finally
			{
				context.Response.OutputStream.Dispose();
			}
		}

		#endregion
	}
}
