using System;
using System.Collections.Generic;
using System.Text;
using Subvert;
using System.Net;
using SvnBridge.Handlers;
using System.Threading;

namespace SvnBridge.RequestReceiver
{
    public class HttpListenerRequestReceiver
    {
        private static HttpListener _listener;

        public static void Start(int portNumber, string tfsServer)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + portNumber.ToString() + "/");
            _listener.Start();
            Thread requestProcessor = new Thread(ReceiveLoop);
            requestProcessor.Start(tfsServer);
        }

        public static void ReceiveLoop(object parameters)
        {
            string tfsServer = (string)parameters;
            RequestHandler handler = new RequestHandler(tfsServer);
            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                handler.ProcessRequest(new HttpListenerHttpRequest(context));
                context.Response.OutputStream.Close();
            }
        }
    }
}
