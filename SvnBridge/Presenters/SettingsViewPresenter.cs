using System.Windows.Forms;
using SvnBridge.Views;

namespace SvnBridge.Presenters
{
    public class SettingsViewPresenter
    {
        private readonly ISettingsView view;
        private bool cancelled;
        private int port;
        private string tfsUrl;

        public SettingsViewPresenter(ISettingsView view)
        {
            this.view = view;

            view.Presenter = this;
        }

        public bool Cancelled
        {
            get
            {
                if (view.DialogResult == DialogResult.Cancel)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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