namespace SvnBridge.Net
{
    public interface IListener
    {
        int Port { get; set; }
        string TfsUrl { get; set; }

        void Start();
        void Stop();
    }
}