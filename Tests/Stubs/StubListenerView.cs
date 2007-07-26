using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Presenters;
using SvnBridge.Views;

namespace SvnBridge.Stubs
{
    public class StubListenerView : IListenerView
    {
        public ListenerViewPresenter Set_Presenter;
        public bool OnListenerStarted_Called;
        public bool OnListenerStopped_Called;
        public bool Show_Called;

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