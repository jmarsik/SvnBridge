using System;
using System.Threading;
using System.Windows.Forms;
using SvnBridge.Net;
using SvnBridge.Presenters;
using SvnBridge.Views;
using SvnBridge.Views.Console;
using SvnBridge.Views.Gui;

namespace SvnBridge
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread"; // TODO: Is this needed?
            
            StartOptions startOptions;

            try
            {
                startOptions = new StartOptions(args);
            }
            catch(StartOptionsException ex)
            {
                DisplayMessage(ex.Message, ex.DisplayGui);
                
                return;
            }

            IListener listener = ListenerFactory.Create();
            if (startOptions.Port > 0)
                listener.Port = startOptions.Port;
            if (!String.IsNullOrEmpty(startOptions.TfsUrl))
                listener.TfsUrl = startOptions.TfsUrl;

            if (startOptions.DisplayGui)
                RunWithGui(listener);
            else
                RunWithConsole(listener);
        }

        private static void DisplayMessage(string message, bool displayInGui)
        {
            if (displayInGui)
                MessageBox.Show(message, "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                Console.WriteLine(message);
        }

        private static void RunWithConsole(IListener listener)
        {
            PrepareView(listener, new ConsoleListenerView(), new ConsoleSettingsView());
        }

        private static void RunWithGui(IListener listener)
        {
            NativeMethods.FreeConsole();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ToolTrayForm listenerView = new ToolTrayForm();
            
            if (PrepareView(listener, listenerView, new SettingsForm()))
                Application.Run(listenerView);
            else
                listenerView.Close();
        }

        private static bool PrepareView(IListener listener, IListenerView listenerView, ISettingsView settingsView)
        {
            ListenerViewPresenter listenerViewPresenter = new ListenerViewPresenter(listenerView, listener);
            SettingsViewPresenter settingsViewPresenter = new SettingsViewPresenter(settingsView);

            if (listener.Port == 0)
            {
                settingsViewPresenter.Show();

                if (!settingsViewPresenter.Cancelled)
                {
                    listener.Port = settingsViewPresenter.Port;
                    listener.TfsUrl = settingsViewPresenter.TfsUrl;
                }
            }

            if (listener.Port == 0)
                return false;
            else
            {
                listenerViewPresenter.Show();
                listenerViewPresenter.StartListener();

                return true;
            }
        }
    }
}