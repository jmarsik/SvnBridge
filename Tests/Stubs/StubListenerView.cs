using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Presenters;
using SvnBridge.Views;

namespace SvnBridge.Stubs
{
    public class StubListenerView : IListenerView
    {
        internal ListenerViewPresenter Set_Presenter;
        internal bool OnListenerStarted_Called;
        internal bool OnListenerStopped_Called;
        internal bool Show_Called;

        public ListenerViewPresenter Presenter
        {
            set { Set_Presenter = value; }
        }

        public void OnListenerStarted()
        {
            OnListenerStarted_Called = true;
        }

        public void OnListenerStopped()
        {
            OnListenerStopped_Called = true;
        }

        public void Show()
        {
            Show_Called = true;
        }
    }
}