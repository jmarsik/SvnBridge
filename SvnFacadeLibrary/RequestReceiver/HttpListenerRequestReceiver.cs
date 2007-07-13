using System.Net;
using System.Threading;
using SvnBridge.Handlers;

namespace SvnBridge.RequestReceiver
{
    public class HttpListenerRequestReceiver : IRequestReceiver
    {
        static HttpListener listener;

        public void Start(int portNumber,
                          string tfsServer)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + portNumber + "/");
            listener.Start();
            Thread requestProcessor = new Thread(ReceiveLoop);
            requestProcessor.Start(tfsServer);
        }

        public void Stop() {}

        public static void ReceiveLoop(object parameters)
        {
            string tfsServer = (string)parameters;
            IRequestDispatcher dispatcher = RequestDispatcherFactory.Create(tfsServer);

            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                try
                {
                    dispatcher.Dispatch(new HttpListenerHttpRequest(context));
                }
                finally
                {
                    context.Response.OutputStream.Close();
                }
            }
        }
    }
}