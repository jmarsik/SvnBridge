using System;
using System.Windows.Forms;

namespace Subvert
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

        void OnOkButtonClicked(object sender,
                            EventArgs e)
        {
            int port;

            if (!int.TryParse(txtPortNumber.Text, out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("The port number does not appear to be valid. Please choose a number between 1 and 65535.", "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPortNumber.Focus();
                txtPortNumber.SelectAll();
                return;
            }

            try
            {
                new Uri(TfsServer);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("The TFS Server URL does not appear to be valid.", "SvnBridge", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtTFSServer.Focus();
                txtTFSServer.SelectAll();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}