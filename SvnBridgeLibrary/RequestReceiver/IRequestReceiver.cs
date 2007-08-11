namespace SvnBridge.RequestReceiver
{
    public interface IRequestReceiver
    {
        int Port { get; set; }
        string TfsServerUrl { get; set; }

        void Start();

        void Stop();
    }
}