using System.Configuration;
using System.Reflection;
using System.Web;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.PathParsing;

namespace SvnBridge.Web
{
	public class SvnBridgeHttpHandler : IHttpHandler
	{
		private readonly HttpContextDispatcher dispatcher;

		public SvnBridgeHttpHandler()
		{
			string tfsUrl = ConfigurationManager.AppSettings["TfsUrl"];
		    PerRequest.Items["serverUrl"] = tfsUrl;
		    IProjectInformationRepository projectInformationRepository = IoC.Resolve<IProjectInformationRepository>();
		    if (ConfigurationManager.AppSettings["URLIncludesProjectName"].ToLower() == "true")
			{
                dispatcher = new HttpContextDispatcher(new StaticServerWithProjectNameInHostNamePathParser(tfsUrl, projectInformationRepository), IoC.Resolve<IActionTracking>());
			}
			else
			{
                dispatcher = new HttpContextDispatcher(new StaticServerPathParser(tfsUrl, projectInformationRepository), IoC.Resolve<IActionTracking>());
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
