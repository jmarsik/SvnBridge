using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Presenters;

namespace SvnBridge.Views.Console
{
    public class ConsoleSettingsView : ISettingsView
    {
        private SettingsViewPresenter presenter;

        public SettingsViewPresenter Presenter
        {
            set { presenter = value; }
        }

        public void Show()
        {
            System.Console.CancelKeyPress += delegate { presenter.Cancelled = true; };

            presenter.Port = GetPort();
            presenter.TfsServerUrl = GetTfsServerUrl();
        }

        private static int GetPort()
        {
            while (true)
            {
                int port;

                System.Console.Write("Enter the listener port: ");

                string portInput = System.Console.ReadLine();

                if (!int.TryParse(portInput, out port))
                    System.Console.WriteLine("Invalid port: must be a number");

                if (port < 1 || port > Constants.MaxPort)
                    System.Console.WriteLine("Invalid port: must be between 1 and {0}", Constants.MaxPort);
                else
                    return port;
            }
        }

        private static string GetTfsServerUrl()
        {
            while (true)
            {
                Uri tfsServerUrl;

                System.Console.Write("Enter the Team Foundation Server URL: ");

                string tfsServerUrlInput = System.Console.ReadLine();

                if (!(Uri.TryCreate(tfsServerUrlInput, UriKind.Absolute, out tfsServerUrl)))
                    System.Console.WriteLine("Invalid Team Foundation Server URL: must be a valid, absolute URL");
                else
                    return tfsServerUrl.ToString();
            }
        }
    }
}
