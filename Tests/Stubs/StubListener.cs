using System;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.PathParsing;

namespace SvnBridge.Stubs
{
    public delegate void StartDelegate();

    public delegate void StopDelegate();

    public class StubListener : IListener
    {
        public int Get_Port;
        public int Set_Port;
        public bool Start_Called;
        public StartDelegate Start_Delegate;
        public bool Stop_Called;
        public StopDelegate Stop_Delegate;

        #region IListener Members

        public event EventHandler<ListenErrorEventArgs> ListenError = delegate { };
        event EventHandler<FinishedHandlingEventArgs> IListener.FinishedHandling
        {
            add { }
            remove { }
        }

        public int Port
        {
            get { return Get_Port; }
            set { Set_Port = value; }
        }

        public void Start(IPathParser parser)
        {
            if (Start_Delegate != null)
            {
                Start_Delegate();
            }

            Start_Called = true;
        }

        public void Stop()
        {
            if (Stop_Delegate != null)
            {
                Stop_Delegate();
            }
            Stop_Called = true;
        }

       
        #endregion

        public bool ListenErrorHasDelegate()
        {
            return ListenError != null;
        }

        public void RaiseListenErrorEvent(string message)
        {
            ListenError(this, new ListenErrorEventArgs(new Exception(message)));
        }
    }
}