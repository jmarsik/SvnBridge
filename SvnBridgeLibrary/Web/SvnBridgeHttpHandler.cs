using System.Configuration;
using System.Reflection;
using System.Web;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.PathParsing;
using SvnBridge.SourceControl;

namespace SvnBridge.Web
{
	public class SvnBridgeHttpHandler : IHttpHandler
	{
		private readonly HttpContextDispatcher dispatcher;

		public SvnBridgeHttpHandler()
		{
			string tfsUrl = ConfigurationManager.AppSettings["TfsUrl"];
		    PerRequest.Items["serverUrl"] = tfsUrl;
		    ProjectInformationRepository projectInformationRepository = Container.Resolve<ProjectInformationRepository>();
		    if (ConfigurationManager.AppSettings["URLIncludesProjectName"].ToLower() == "true")
			{
                dispatcher = new HttpContextDispatcher(new StaticServerWithProjectNameInHostNamePathParser(tfsUrl, projectInformationRepository), Container.Resolve<ActionTrackingViaPerfCounter>());
			}
			else
			{
                dispatcher = new HttpContextDispatcher(new StaticServerPathParser(tfsUrl, projectInformationRepository), Container.Resolve<ActionTrackingViaPerfCounter>());
			}
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
