using SvnBridge.Views;

namespace SvnBridge.Presenters
{
    public class SettingsViewPresenter
    {
        private bool cancelled;
        private int port;
        private string tfsServerUrl;
        private readonly ISettingsView view;
        
        public SettingsViewPresenter(ISettingsView view)
        {
            this.view = view;

            view.Presenter = this;
        }

        public bool Cancelled
        {
            get { return cancelled; }
            set { cancelled = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string TfsServerUrl
        {
            get { return tfsServerUrl; }
            set { tfsServerUrl = value; }
        }
        
        public void Show()
        {
            view.Show();
        }
    }
}
