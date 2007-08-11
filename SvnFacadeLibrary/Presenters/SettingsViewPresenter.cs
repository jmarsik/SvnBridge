using SvnBridge.Views;

namespace SvnBridge.Presenters
{
    public class SettingsViewPresenter
    {
        private bool cancelled;
        private int port;
        private string tfsUrl;
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

        public string TfsUrl
        {
            get { return tfsUrl; }
            set { tfsUrl = value; }
        }
        
        public void Show()
        {
            view.Show();
        }
    }
}
