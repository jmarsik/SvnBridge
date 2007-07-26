using SvnBridge.Net;
using SvnBridge.Views;

namespace SvnBridge.Presenters
{
    public class ListenerViewPresenter
    {
        private readonly IListener listener;
        private readonly IListenerView view;

        public ListenerViewPresenter(IListenerView view, IListener listener)
        {
            this.listener = listener;
            this.view = view;

            view.Presenter = this;
        }

        public int Port
        {
            get { return listener.Port; }
        }

        public string TfsServerUrl
        {
            get { return listener.TfsServerUrl; }
        }

        public void ChangeSettings(ISettingsView settingsView)
        {
            SettingsViewPresenter settingsViewPresenter = new SettingsViewPresenter(settingsView);

            settingsViewPresenter.Show();

            if ((!settingsViewPresenter.Cancelled) && (SettingsHaveChanged(settingsViewPresenter.Port, settingsViewPresenter.TfsServerUrl)))
            {
                ApplyNewSettings(settingsViewPresenter.Port, settingsViewPresenter.TfsServerUrl);
            }
        }

        public void Show()
        {
            view.Show();
        }

        public void StartListener()
        {
            listener.Start();

            view.OnListenerStarted();
        }

        public void StopListener()
        {
            listener.Stop();

            view.OnListenerStopped();
        }

        private void ApplyNewSettings(int port, string tfsServerUrl)
        {
            StopListener();

            listener.Port = port;
            listener.TfsServerUrl = tfsServerUrl;

            StartListener();
        }

        private bool SettingsHaveChanged(int port, string tfsServerUrl)
        {
            return port != listener.Port || tfsServerUrl != listener.TfsServerUrl;
        }
    }
}