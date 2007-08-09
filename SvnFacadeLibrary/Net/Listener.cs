using System;
using System.Net;
using System.Net.Sockets;
using SvnBridge.Handlers;

namespace SvnBridge.Net
{
    public class Listener : IListener
    {
        private readonly HttpContextDispatcher dispatcher;
        private bool isListening;
        private TcpListener listener;
        private int? port;

        public Listener() : this(null, null)
        {
        }

        public Listener(int port) : this((int?) port, null)
        {
        }

        public Listener(int port, string tfsServerUrl) : this((int?) port, tfsServerUrl)
        {
        }

        private Listener(int? port, string tfsServerUrl)
        {
            this.port = port;

            dispatcher = new HttpContextDispatcher();
            dispatcher.TfsServerUrl = tfsServerUrl;
        }

        #region IListener Members

        public int Port
        {
            get { return port.GetValueOrDefault(); }
            set
            {
                if (isListening)
                    throw new InvalidOperationException("The port cannot be changed while the listener is listening.");

                port = value;
            }
        }

        public string TfsServerUrl
        {
            get { return dispatcher.TfsServerUrl; }
            set
            {
                if (isListening)
                    throw new InvalidOperationException("The TFS server URL cannot be changed while the listener is listening.");

                Uri validUri;
                if (!Uri.TryCreate(value, UriKind.Absolute, out validUri))
                    throw new UriFormatException();

                dispatcher.TfsServerUrl = value;
            }
        }

        public void Start()
        {
            if (!port.HasValue)
                throw new InvalidOperationException("A port must be specified before starting the listener.");

            if (string.IsNullOrEmpty(TfsServerUrl))
                throw new InvalidOperationException("A TFS server URL must be specified before starting the listener.");

            isListening = true;

            listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();

            listener.BeginAcceptTcpClient(Accept, null);
        }

        public void Stop()
        {
            listener.Stop();

            isListening = false;
        }

        #endregion

        private void Accept(IAsyncResult asyncResult)
        {
            TcpClient tcpClient;

            try
            {
                tcpClient = listener.EndAcceptTcpClient(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            listener.BeginAcceptTcpClient(Accept, null);

            if (tcpClient != null)
                Process(tcpClient);
        }

        private void Process(TcpClient tcpClient)
        {
            IHttpContext connection = new ListenerContext(tcpClient.GetStream());

            dispatcher.Dispatch(connection);

            connection.Response.OutputStream.Flush();

            tcpClient.Close();
        }
    }
}