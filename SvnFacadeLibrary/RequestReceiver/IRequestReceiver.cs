namespace SvnBridge.RequestReceiver
{
    public interface IRequestReceiver
    {
        // Methods

        void Start(int portNumber,
                   string tfsServer);

        void Stop();
    }
}