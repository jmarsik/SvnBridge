using System;
using System.Windows.Forms;
using SvnBridge.Utility;

namespace SvnBridge
{
    public partial class SettingsForm : Form
    {
        // Lifetime

        public SettingsForm()
        {
            InitializeComponent();
        }

        // Properties

        public int PortNumber
        {
            get { return int.Parse(txtPortNumber.Text); }
            set { txtPortNumber.Text = value.ToString(); }
        }

        public string TfsServer
        {
            get { return txtTFSServer.Text; }
            set { txtTFSServer.Text = value; }
        }

        // Methods

        void OnOkButtonClicked(object sender,
                               EventArgs e)
        {
            if (!Helper.IsValidPort(txtPortNumber.Text))
            {
                MessageBox.Show("The port number does not appear to be valid. Please choose a number between 1 and 65535.", "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPortNumber.Focus();
                txtPortNumber.SelectAll();
                return;
            }

            if (!Helper.IsValidUrl(TfsServer))
            {
                MessageBox.Show("The TFS Server URL does not appear to be valid.", "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTFSServer.Focus();
                txtTFSServer.SelectAll();
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            bool validTfsUrl = Helper.IsValidTFSUrl(TfsServer);
            this.Cursor = Cursors.Default;
            if (!validTfsUrl)
            {
                MessageBox.Show("The TFS Server URL does not appear to a TFS server.", "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTFSServer.Focus();
                txtTFSServer.SelectAll();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}