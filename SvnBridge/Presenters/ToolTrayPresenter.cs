using SvnBridge.RequestReceiver;

namespace SvnBridge
{
    public class ToolTrayPresenter
    {
        // Fields

        ITcpClientRequestReceiver receiver;
        IToolTrayView view;
        int currentPortNumber = 8081;
        string currentTfsServer = string.Empty;

        // Lifetime

        public ToolTrayPresenter(IToolTrayView view,
                                 ITcpClientRequestReceiver receiver)
        {
            this.receiver = receiver;
            this.view = view;
        }

        // Properties

        public int PortNumber
        {
            get { return currentPortNumber; }
        }

        public string TfsServer
        {
            get { return currentTfsServer; }
        }

        // Methods

        public void Start(int portNumber,
                          string tfsServer)
        {
            receiver.Start(portNumber, tfsServer);

            currentPortNumber = portNumber;
            currentTfsServer = tfsServer;

            view.OnServerStarted();
        }

        public void Stop()
        {
            receiver.Stop();

            view.OnServerStopped();
        }
    }
}