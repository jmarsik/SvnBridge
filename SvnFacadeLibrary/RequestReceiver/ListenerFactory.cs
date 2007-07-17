namespace SvnBridge.RequestReceiver
{
    public static class ListenerFactory
    {
        public static IRequestReceiver Create()
        {
            return new TcpClientRequestReceiver();
        }
    }
}