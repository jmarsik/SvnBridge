using System.Net;
using System.Threading;
using SvnBridge.Handlers;

namespace SvnBridge.RequestReceiver
{
    public class HttpListenerRequestReceiver : IRequestReceiver
    {
        private static HttpListener listener;
        private int port;
        private string tfsServerUrl;

        #region IRequestReceiver Members

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + Port + "/");
            listener.Start();
            Thread requestProcessor = new Thread(ReceiveLoop);
            requestProcessor.Start(tfsServerUrl);
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string TfsServerUrl
        {
            get { return tfsServerUrl; }
            set { tfsServerUrl = value; }
        }

        public void Stop()
        {
        }

        #endregion

        public static void ReceiveLoop(object parameters)
        {
            string tfsServer = (string) parameters;
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