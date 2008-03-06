using System;
using System.Security.Cryptography;
using System.Text;
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
            {
                tfsUrl = args[0];
            }

            if (args.Length > 1)
            {
                int tmp;
                if (int.TryParse(args[1], out tmp))
                {
                    port = tmp;
                }
            }
            ProxyInformation proxyInfo = GetProxyInfo();

            if (!String.IsNullOrEmpty(tfsUrl) || TryGetSettings(ref tfsUrl, ref port, proxyInfo))
            {
                Run(tfsUrl, port ?? 8081, proxyInfo);
            }
        }

        private static ProxyInformation GetProxyInfo()
        {
            ProxyInformation proxyInfo = new ProxyInformation();
            proxyInfo.UseProxy = Settings.Default.UseProxy;
            proxyInfo.Url = Settings.Default.ProxyUrl;
            proxyInfo.Port = Settings.Default.ProxyPort;
            proxyInfo.UseDefaultCredentails = Settings.Default.ProxyUseDefaultCredentials;
            proxyInfo.Username = Settings.Default.ProxyUsername;

            if (Settings.Default.ProxyEncryptedPassword != null)
            {
                byte[] password = ProtectedData.Unprotect(
                    Settings.Default.ProxyEncryptedPassword,
                    Encoding.UTF8.GetBytes("ProxyEncryptedPassword"),
                    DataProtectionScope.CurrentUser
                    );
                proxyInfo.Password = Encoding.UTF8.GetString(password);
            }
            return proxyInfo;
        }

        private static bool TryGetSettings(ref string tfsUrl,
                                           ref int? port,
                                           ProxyInformation proxyInfo)
        {
            SettingsForm view = new SettingsForm();
            SettingsViewPresenter presenter = new SettingsViewPresenter(view,proxyInfo);
            presenter.TfsUrl = tfsUrl ?? Settings.Default.TfsUrl;
            presenter.Port = port ?? Settings.Default.TfsPort;
            presenter.Show();


            if (!presenter.Cancelled)
            {
                tfsUrl = Settings.Default.TfsUrl = presenter.TfsUrl;
                port = Settings.Default.TfsPort = presenter.Port;

                Settings.Default.UseProxy = proxyInfo.UseProxy;
                Settings.Default.ProxyUrl = proxyInfo.Url;
                Settings.Default.ProxyPort = proxyInfo.Port;
                Settings.Default.ProxyUseDefaultCredentials = proxyInfo.UseDefaultCredentails;
                Settings.Default.ProxyUsername = proxyInfo.Username;

                byte[] password = null;
                if (proxyInfo.Password != null)
                {
                    password = ProtectedData.Protect(
                        Encoding.UTF8.GetBytes(proxyInfo.Password),
                        Encoding.UTF8.GetBytes("ProxyEncryptedPassword"),
                        DataProtectionScope.CurrentUser
                        );
                }
                Settings.Default.ProxyEncryptedPassword = password;

                Settings.Default.Save();
            }
            return !presenter.Cancelled;
        }

        private static void Run(string tfsUrl, int port, ProxyInformation proxyInformation)
        {
            ListenerFactory.SetProxy(proxyInformation);
            IListener listener = ListenerFactory.Create();

            listener.TfsUrl = tfsUrl;
            listener.Port = port;

            ToolTrayForm view = new ToolTrayForm();
            ListenerViewPresenter presenter = new ListenerViewPresenter(view, listener);

            try
            {
                presenter.Show();
                presenter.StartListener();

                Application.Run(view);
            }
            finally
            {
                presenter.StopListener();
            }
        }
    }
}