using System;
using System.Threading;
using System.Windows.Forms;
using SvnBridge.Presenters;
using SvnBridge.RequestReceiver;
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

            IRequestReceiver listener = ListenerFactory.Create();
            listener.Port = startOptions.Port;
            listener.TfsServerUrl = startOptions.TfsServerUrl;

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

        private static void RunWithConsole(IRequestReceiver listener)
        {
            PrepareView(listener, new ConsoleListenerView(), new ConsoleSettingsView());
        }

        private static void RunWithGui(IRequestReceiver listener)
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

        private static bool PrepareView(IRequestReceiver listener, IListenerView listenerView, ISettingsView settingsView)
        {
            ListenerViewPresenter listenerViewPresenter = new ListenerViewPresenter(listenerView, listener);
            SettingsViewPresenter settingsViewPresenter = new SettingsViewPresenter(settingsView);

            if (listener.Port == 0)
            {
                settingsViewPresenter.Show();

                listener.Port = settingsViewPresenter.Port;
                listener.TfsServerUrl = settingsViewPresenter.TfsServerUrl;
            }

            if (settingsViewPresenter.Cancelled)
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