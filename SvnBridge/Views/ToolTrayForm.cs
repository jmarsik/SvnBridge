using System;
using System.Diagnostics;
using System.Windows.Forms;
using SvnBridge.Presenters;

namespace SvnBridge.Views
{
    public partial class ToolTrayForm : Form, IListenerView
    {
        private ListenerViewPresenter presenter;

        public ToolTrayForm()
        {
            InitializeComponent();
        }

        #region IListenerView Members

        public void OnListenerStarted()
        {
            notifyIcon.Text = "Running: http://localhost:" + presenter.Port;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;

            notifyIcon.ShowBalloonTip(500,
                                      "SvnBridge",
                                      "Started on port " + presenter.Port + "\r\nForwarding to " +
                                      presenter.TfsUrl,
                                      ToolTipIcon.Info);
        }

        public void OnListenerStopped()
        {
            notifyIcon.BalloonTipText = "Not running";
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;

            notifyIcon.ShowBalloonTip(500, "SvnBridge", "Stopped", ToolTipIcon.Info);
        }

        public void OnListenerError(string message)
        {
            Trace.TraceError(message);
            notifyIcon.ShowBalloonTip(1000, "SvnBridge", message, ToolTipIcon.Error);
        }

        public ListenerViewPresenter Presenter
        {
            set { presenter = value; }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            presenter.StopListener();
        }

        private void OnExitClicked(object sender,
                                   EventArgs e)
        {
            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            Hide();
        }

        private void OnSettingsClicked(object sender,
                                       EventArgs e)
        {
            SettingsForm settingsView = new SettingsForm();

            presenter.ChangeSettings(settingsView);
        }

        private void OnStartClicked(object sender,
                                    EventArgs e)
        {
            presenter.StartListener();
        }

        private void OnStopClicked(object sender,
                                   EventArgs e)
        {
            presenter.StopListener();
        }

        public void Run()
        {
            presenter.StartListener();
        }
    }
}