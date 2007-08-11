using SvnBridge.Net;

namespace SvnBridge.Stubs
{
    public delegate void StartDelegate();
    public delegate void StopDelegate();

    public class StubListener : IListener
    {
        public int Get_Port;
        public int Set_Port;
        public string Get_TfsUrl;
        public string Set_TfsUrl;
        public bool Start_Called;
        public StartDelegate Start_Delegate;
        public bool Stop_Called;
        public StopDelegate Stop_Delegate;

        public int Port
        {
            get { return Get_Port; }
            set { Set_Port = value; }
        }

        public string TfsUrl
        {
            get { return Get_TfsUrl; }
            set { Set_TfsUrl = value; }
        }

        public void Start()
        {
            if (Start_Delegate != null)
                Start_Delegate();

            Start_Called = true;
        }

        public void Stop()
        {
            if (Stop_Delegate != null)
                Stop_Delegate();

            Stop_Called = true;
        }
    }
}
