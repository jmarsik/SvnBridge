using System;
using System.Windows.Forms;
using SvnBridge.Net;
using SvnBridge.Presenters;
using SvnBridge.Views;

namespace SvnBridge
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            string tfsUrl = null;
            int port = 8081;

            if (args.Length > 0)
                tfsUrl = args[0];

            if (args.Length > 1)
                int.TryParse(args[1], out port);

            if (!String.IsNullOrEmpty(tfsUrl) || TryGetSettings(ref tfsUrl, ref port))
                Run(tfsUrl, port);
        }

        private static bool TryGetSettings(ref string tfsUrl, ref int port)
        {
            SettingsForm view = new SettingsForm();
            SettingsViewPresenter presenter = new SettingsViewPresenter(view);

            presenter.Show();

            if (!presenter.Cancelled)
            {
                tfsUrl = presenter.TfsUrl;
                port = presenter.Port;
            }

            return !presenter.Cancelled;
        }

        private static void Run(string tfsUrl, int port)
        {
            IListener listener = ListenerFactory.Create();

            listener.TfsUrl = tfsUrl;
            listener.Port = port;

            ToolTrayForm view = new ToolTrayForm();
            ListenerViewPresenter presenter = new ListenerViewPresenter(view, listener);

            presenter.Show();
            presenter.StartListener();

            Application.Run(view);
        }
    }
}