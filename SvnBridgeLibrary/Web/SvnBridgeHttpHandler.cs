using System.Configuration;
using System.Reflection;
using System.Web;
using SvnBridge.Infrastructure;
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
			if (ConfigurationManager.AppSettings["URLIncludesProjectName"].ToLower() == "true")
			{
				dispatcher = new HttpContextDispatcher(new StaticServerWithProjectNameInHostNamePathParser(tfsUrl));
			}
			else
			{
				dispatcher = new HttpContextDispatcher(new StaticServerPathParser(tfsUrl));
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
