using System;
using System.Windows.Forms;
using SvnBridge.Presenters;
using SvnBridge.Utility;

namespace SvnBridge.Views
{
    public partial class SettingsForm : Form, ISettingsView
    {
        private SettingsViewPresenter presenter;

        public SettingsForm()
        {
            InitializeComponent();
        }

        #region ISettingsView Members

        public SettingsViewPresenter Presenter
        {
            set { presenter = value; }
        }

        public new void Show()
        {
            txtPortNumber.Text = presenter.Port.ToString();
            txtTfsUrl.Text = presenter.TfsUrl;
            ShowDialog();
        }

        #endregion

        private void OnOkButtonClicked(object sender,
                                       EventArgs e)
        {
            if (!Helper.IsValidPort(txtPortNumber.Text))
            {
                MessageBox.Show(
                    "The port number does not appear to be valid. Please choose a number between 1 and 65535.",
                    "SvnBridge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                txtPortNumber.Focus();
                txtPortNumber.SelectAll();
                return;
            }

            if (Helper.IsPortInUse(int.Parse(txtPortNumber.Text)))
            {
                MessageBox.Show(
                    "The port number appears to already be in use. Please choose a different port.",
                    "SvnBridge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                txtPortNumber.Focus();
                txtPortNumber.SelectAll();
                return;
            }

            if (!Helper.IsValidUrl(txtTfsUrl.Text))
            {
                MessageBox.Show("The TFS Server URL does not appear to be valid.",
                                "SvnBridge",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                txtTfsUrl.Focus();
                txtTfsUrl.SelectAll();
                return;
            }

            Cursor = Cursors.WaitCursor;
            bool validTfsUrl = Helper.IsValidTFSUrl(txtTfsUrl.Text);
            Cursor = Cursors.Default;
            if (!validTfsUrl)
            {
                MessageBox.Show("The TFS Server URL does not appear to a TFS server.",
                                "SvnBridge",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                txtTfsUrl.Focus();
                txtTfsUrl.SelectAll();
                return;
            }

            presenter.Port = int.Parse(txtPortNumber.Text);
            presenter.TfsUrl = txtTfsUrl.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnCancelButtonClicked(object sender,
                                           EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}