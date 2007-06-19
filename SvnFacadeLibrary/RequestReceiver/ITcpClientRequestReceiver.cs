namespace SvnBridge.RequestReceiver
{
    public interface ITcpClientRequestReceiver
    {
        // Methods

        void Start(int portNumber,
                   string tfsServer);

        void Stop();
    }
}