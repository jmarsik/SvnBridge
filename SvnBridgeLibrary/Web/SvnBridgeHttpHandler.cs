using System.Configuration;
using System.Web;
using SvnBridge.Net;

namespace SvnBridge.Web
{
    public class SvnBridgeHttpHandler : IHttpHandler
    {
        private readonly HttpContextDispatcher dispatcher;

        public SvnBridgeHttpHandler()
        {
            dispatcher = new HttpContextDispatcher();
            dispatcher.TfsUrl = ConfigurationManager.AppSettings["TfsUrl"];
            if (ConfigurationManager.AppSettings["URLIncludesProjectName"].ToLower() == "true")
            {
                dispatcher.URLIncludesProjectName = true;
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