using System.Web;

namespace SvnBridge.Handlers
{
    public class IISRequestReceiver : IHttpHandler
    {
        IRequestDispatcher _dispatcher = RequestDispatcherFactory.Create(null);

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            _dispatcher.Dispatch(new IISHttpRequest(context));
        }
    }
}