using System;
using System.Threading;
using System.Windows.Forms;
using SvnBridge.RequestReceiver;
using SvnBridge.Utility;

namespace SvnBridge
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int portNumber = -1;
            string tfsServer = null;

            if (args.Length >= 2)
            {
                if (Helper.IsValidPort(args[0]) && Helper.IsValidUrl(args[1]))
                {
                    portNumber = int.Parse(args[0]);
                    tfsServer = args[1];
                }
            }

            if (portNumber < 0)
            {
                SettingsForm settings = new SettingsForm();
                DialogResult result = settings.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                portNumber = settings.PortNumber;
                tfsServer = settings.TfsServer;
            }

            //ToolTrayForm toolTray = new ToolTrayForm(new HttpListenerRequestReceiver());
            ToolTrayForm toolTray = new ToolTrayForm(new TcpClientRequestReceiver());
            toolTray.Run(portNumber, tfsServer);
            Application.Run(toolTray);
        }
    }
}