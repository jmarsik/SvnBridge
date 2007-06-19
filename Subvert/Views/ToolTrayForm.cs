using System;
using System.Windows.Forms;
using SvnBridge.RequestReceiver;

namespace Subvert
{
    public partial class ToolTrayForm : Form, IToolTrayView
    {
        // Fields

        ToolTrayPresenter presenter;

        // Lifetime

        public ToolTrayForm(ITcpClientRequestReceiver receiver)
        {
            InitializeComponent();
            presenter = new ToolTrayPresenter(this, receiver);
        }

        // Methods

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            presenter.Stop();
        }

        void OnExitClicked(object sender,
                           EventArgs e)
        {
            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            Hide();
        }

        void OnSettingsClicked(object sender,
                               EventArgs e)
        {
            ShowSettings();
        }

        void OnStartClicked(object sender,
                            EventArgs e)
        {
            presenter.Start(presenter.PortNumber, presenter.TfsServer);
        }

        void OnStopClicked(object sender,
                           EventArgs e)
        {
            presenter.Stop();
        }

        public void OnServerStarted()
        {
            notifyIcon.Text = "Running: http://localhost:" + presenter.PortNumber;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            notifyIcon.ShowBalloonTip(500, "SvnBridge", "Started on port " + presenter.PortNumber + "\r\nForwarding to " + presenter.TfsServer, ToolTipIcon.Info);
        }

        public void OnServerStopped()
        {
            notifyIcon.BalloonTipText = "Not running";
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;

            notifyIcon.ShowBalloonTip(500, "SvnBridge", "Stopped", ToolTipIcon.Info);
        }

        public void Run(int portNumber,
                        string tfsServer)
        {
            presenter.Start(portNumber, tfsServer);
        }

        void ShowSettings()
        {
            SettingsForm settings = new SettingsForm();
            settings.PortNumber = presenter.PortNumber;
            settings.TfsServer = presenter.TfsServer;

            if (settings.ShowDialog() == DialogResult.OK)
            {
                presenter.Stop();
                presenter.Start(settings.PortNumber, settings.TfsServer);
            }
        }
    }
}