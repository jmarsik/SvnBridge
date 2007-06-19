using System;
using System.Threading;
using System.Windows.Forms;
using SvnBridge.RequestReceiver;

namespace Subvert
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.Name = "Main Thread";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SettingsForm settings = new SettingsForm();
            DialogResult result = settings.ShowDialog();

            if (result != DialogResult.OK)
                return;

            ToolTrayForm toolTray = new ToolTrayForm(new TcpClientRequestReceiver());
            toolTray.Run(settings.PortNumber, settings.TfsServer);
            Application.Run(toolTray);
        }
    }
}