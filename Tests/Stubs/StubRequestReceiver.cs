using SvnBridge.RequestReceiver;

namespace Tests.SvnBridge
{
    class StubRequestReceiver : IRequestReceiver
    {
        public int Start_PortNumber;
        public string Start_TfsServer;

        public bool Stop_Called;

        public void Start(int portNumber,
                          string tfsServer)
        {
            Start_PortNumber = portNumber;
            Start_TfsServer = tfsServer;
        }

        public void Stop()
        {
            Stop_Called = true;
        }
    }
}