using System;
using System.Windows.Forms;
using SvnBridge.Presenters;
using SvnBridge.Utility;
using SvnBridge.Views;

namespace SvnBridge.Views.Gui
{
    public partial class SettingsForm : Form, ISettingsView
    {
        private SettingsViewPresenter presenter;
        
        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsViewPresenter Presenter
        {
            set { presenter = value; }
        }

        public new void Show()
        {
            ShowDialog();
        }

        private void OnOkButtonClicked(object sender, EventArgs e)
        {
            if (!Helper.IsValidPort(txtPortNumber.Text))
            {
                MessageBox.Show(
                    "The port number does not appear to be valid. Please choose a number between 1 and 65535.",
                    "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPortNumber.Focus();
                txtPortNumber.SelectAll();
                return;
            }

            if (!Helper.IsValidUrl(txtTFSServer.Text))
            {
                MessageBox.Show("The TFS Server URL does not appear to be valid.", "SvnBridge", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                txtTFSServer.Focus();
                txtTFSServer.SelectAll();
                return;
            }

            Cursor = Cursors.WaitCursor;
            bool validTfsUrl = Helper.IsValidTFSUrl(txtTFSServer.Text);
            Cursor = Cursors.Default;
            if (!validTfsUrl)
            {
                MessageBox.Show("The TFS Server URL does not appear to a TFS server.", "SvnBridge", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                txtTFSServer.Focus();
                txtTFSServer.SelectAll();
                return;
            }

            presenter.Port = int.Parse(txtPortNumber.Text);
            presenter.TfsServerUrl = txtTFSServer.Text;
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancelButtonClicked(object sender, EventArgs e)
        {
            presenter.Cancelled = true;

            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}