using System;
using System.Windows.Forms;
using SvnBridge.Presenters;
using SvnBridge.Utility;

namespace SvnBridge.Views
{
	public partial class SettingsForm : Form, ISettingsView
	{
		private SettingsViewPresenter presenter;
		private readonly ProxySettings proxySettings = new ProxySettings();

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
			if (presenter.GetServerUrlFromRequest!=null)
				GetServerUrlFromRequest.Checked = presenter.GetServerUrlFromRequest.Value;
			else
				GetServerUrlFromRequest.Checked = true;
			ShowDialog();
		}

		#endregion

		private void OnProxyButtonClicked(object sender, EventArgs e)
		{
			proxySettings.SetInformation(presenter.ProxyInformation);
			if (proxySettings.ShowDialog(this) != DialogResult.OK)
				return;
			presenter.ProxyInformation.UseProxy = string.IsNullOrEmpty(proxySettings.ProxyUrl) == false;
			presenter.ProxyInformation.Url = proxySettings.ProxyUrl;
			presenter.ProxyInformation.Port = proxySettings.Port;
			presenter.ProxyInformation.Username = proxySettings.Username;
			presenter.ProxyInformation.Password = proxySettings.Password;
			presenter.ProxyInformation.UseDefaultCredentails = proxySettings.UseDefaultCredentials;
		}

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

			int portNumber = int.Parse(txtPortNumber.Text);
			if (presenter.IgnoredUsedPort != portNumber && Helper.IsPortInUseOnLocalHost(portNumber))
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
			bool validTfsUrl = 
				GetServerUrlFromRequest.Checked ||
				Helper.IsValidTFSUrl(txtTfsUrl.Text, presenter.ProxyInformation);

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
			presenter.GetServerUrlFromRequest = GetServerUrlFromRequest.Checked;

			DialogResult = DialogResult.OK;
			Close();
		}

		private void OnCancelButtonClicked(object sender,
										   EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void GetServerUrlFromRequest_CheckedChanged(object sender, EventArgs e)
		{
			txtTfsUrl.Enabled = GetServerUrlFromRequest.Checked == false;
		}
	}
}
