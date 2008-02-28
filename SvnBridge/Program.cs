using System;
using System.Windows.Forms;
using SvnBridge.Net;
using SvnBridge.Presenters;
using SvnBridge.Properties;
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

            new BootStrapper().Start();

            string tfsUrl = null;
            int? port = null;

            if (args.Length > 0)
                tfsUrl = args[0];

            if (args.Length > 1)
            {
                int tmp;
                if (int.TryParse(args[1], out tmp))
                    port = tmp;
            }

            if (!String.IsNullOrEmpty(tfsUrl) || TryGetSettings(ref tfsUrl, ref port))
                Run(tfsUrl, port ?? 8081);
        }

        private static bool TryGetSettings(ref string tfsUrl, ref int? port)
        {
            SettingsForm view = new SettingsForm();
            SettingsViewPresenter presenter = new SettingsViewPresenter(view);
            presenter.TfsUrl = tfsUrl ?? Settings.Default.TfsUrl;
            presenter.Port = port ?? Settings.Default.TfsPort;
            presenter.Show();

            if (!presenter.Cancelled)
            {
                tfsUrl = Settings.Default.TfsUrl   = presenter.TfsUrl;
                port   = Settings.Default.TfsPort  = presenter.Port;
                Settings.Default.Save();
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