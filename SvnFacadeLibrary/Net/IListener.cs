namespace SvnBridge.Net
{
    public interface IListener
    {
        int Port { get; set; }
        string TfsServerUrl { get; set; }

        void Start();
        void Stop();
    }
}