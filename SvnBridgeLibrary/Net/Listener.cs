using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SvnBridge.Net
{
    public class Listener : IListener
    {
        private readonly HttpContextDispatcher dispatcher;
        private bool isListening;
        private TcpListener listener;
        private int? port;

        public Listener()
            : this(null, null)
        {
        }

        public Listener(int port)
            : this((int?)port, null)
        {
        }

        public Listener(int port,
                        string tfsUrl)
            : this((int?)port, tfsUrl)
        {
        }

        private Listener(int? port,
                         string tfsUrl)
        {
            this.port = port;

            dispatcher = new HttpContextDispatcher();
            dispatcher.TfsUrl = tfsUrl;
        }

        #region IListener Members

        public event EventHandler<ListenErrorEventArgs> ListenError = delegate { };

        public int Port
        {
            get { return port.GetValueOrDefault(); }
            set
            {
                if (isListening)
                {
                    throw new InvalidOperationException("The port cannot be changed while the listener is listening.");
                }

                port = value;
            }
        }

        public string TfsUrl
        {
            get { return dispatcher.TfsUrl; }
            set
            {
                if (isListening)
                {
                    throw new InvalidOperationException(
                        "The TFS server URL cannot be changed while the listener is listening.");
                }

                // validate URI
                new Uri(value, UriKind.Absolute);

                dispatcher.TfsUrl = value;
            }
        }

        public void Start()
        {
            if (!port.HasValue)
            {
                throw new InvalidOperationException("A port must be specified before starting the listener.");
            }

            if (string.IsNullOrEmpty(TfsUrl))
            {
                throw new InvalidOperationException("A TFS server URL must be specified before starting the listener.");
            }

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

            try
            {
                if (tcpClient != null)
                {
                    Process(tcpClient);
                }
            }
            catch (Exception ex)
            {
                OnListenException(ex);
            }
        }

        private void Process(TcpClient tcpClient)
        {
            IHttpContext connection = new ListenerContext(tcpClient.GetStream());
            try
            {
                try
                {
                    dispatcher.Dispatch(connection);
                }
                catch (Exception exception)
                {
                    connection.Response.StatusCode = 500;
                    using (StreamWriter sw = new StreamWriter(connection.Response.OutputStream))
                    {
                        Guid guid = Guid.NewGuid();

                        string message = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                   "<D:error xmlns:D=\"DAV:\" xmlns:m=\"http://apache.org/dav/xmlns\" xmlns:C=\"svn:\">\n" +
                                   "<C:error/>\n" +
                                   "<m:human-readable errcode=\"160024\">\n" +
                                   "Failed to process a request. Failure id: " + guid + Environment.NewLine +
                                   exception +
                                   "</m:human-readable>\n" +
                                   "</D:error>\n";
                        sw.Write(message);

                        LogError(guid, exception);
                    }
                    throw;
                }

                try
                {
                    connection.Response.OutputStream.Flush();
                }
                catch (IOException)
                {
                    /* Ignore error, caused by client cancelling operation */
                }
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private static void LogError(Guid guid, Exception e)
        {
            EventLog.WriteEntry("SvnBridge",
                "Error on handling request. Error id: " + guid + Environment.NewLine + e.ToString(),
                EventLogEntryType.Error);
        }

        private void OnListenException(Exception ex)
        {
            ListenError(this, new ListenErrorEventArgs(ex));
        }
    }
}