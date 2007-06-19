using System.Web;

namespace SvnBridge.Handlers
{
    public class IISRequestReceiver : IHttpHandler
    {
        RequestHandler _handler = new RequestHandler(null);

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            _handler.ProcessRequest(new IISHttpRequest(context));
        }
    }
}