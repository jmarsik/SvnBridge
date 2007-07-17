using SvnBridge.Presenters;

namespace SvnBridge.Views.Console
{
    public class ConsoleListenerView : IListenerView
    {
        private ListenerViewPresenter presenter;

        public ListenerViewPresenter Presenter
        {
            set { presenter = value; }
        }

        public void OnListenerStarted()
        {
            System.Console.WriteLine("Listening on port {0}", presenter.Port); // TODO: localize
            System.Console.WriteLine("Forwarding requests to {0}", presenter.TfsServerUrl); // TODO: localize
            System.Console.WriteLine("Press CTRL + C to stop listening"); // TODO: localize
        }

        public void OnListenerStopped()
        {
            System.Console.WriteLine("Listener stopped"); // TODO: localize
            System.Console.WriteLine("Exiting"); // TODO: localize
        }

        public void Show()
        {
            System.Console.CancelKeyPress += delegate { presenter.StopListener(); };
        }
    }
}
