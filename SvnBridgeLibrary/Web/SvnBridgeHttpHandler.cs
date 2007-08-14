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
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            dispatcher.Dispatch(new HttpContextWrapper(context));
            
            context.Response.OutputStream.Close();
        }

        #endregion
    }
}
