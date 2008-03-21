using System.Windows.Forms;
using SvnBridge.Net;
using SvnBridge.Views;

namespace SvnBridge.Presenters
{
    public class SettingsViewPresenter
    {
        readonly ProxyInformation proxyInformation;

        public ProxyInformation ProxyInformation
        {
            get { return proxyInformation; }
        }

        private readonly ISettingsView view;
        private int port;
        private string tfsUrl;
    	private int ignoredUsedPort;

    	public SettingsViewPresenter(ISettingsView view, ProxyInformation proxyInformation)
        {
            this.view = view;
            this.proxyInformation = proxyInformation;

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

    	public int IgnoredUsedPort
    	{
    		get { return ignoredUsedPort; }
    		set { ignoredUsedPort = value; }
    	}

    	public void Show()
        {
            view.Show();
        }
    }
}