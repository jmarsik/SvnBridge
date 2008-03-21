using SvnBridge.Net;
using SvnBridge.Views;

namespace SvnBridge.Presenters
{
	public class ListenerViewPresenter
	{
		private readonly IListener listener;
		private readonly IListenerView view;
		private readonly IListenerErrorsView errorsView;
		private bool closed;

		public ListenerViewPresenter(IListenerView view,
									 IListenerErrorsView errorsView,
		                             IListener listener)
		{
			this.listener = listener;
			this.view = view;
			this.errorsView = errorsView;
			view.Presenter = this;
			errorsView.Presenter = this;

			listener.ListenError += OnListenError;
		}

		private void OnListenError(object sender, ListenErrorEventArgs e)
		{
			errorsView.AddError(e.Exception.Message, e.Exception.ToString());
			view.OnListenerError(e.Exception.Message);
		}

		public int Port
		{
			get { return listener.Port; }
		}

		public string TfsUrl
		{
			get { return listener.TfsUrl; }
		}

		public bool ShouldCloseErrorView
		{
			get { return closed; }
		}

		public void ChangeSettings(ISettingsView settingsView)
		{
			SettingsViewPresenter settingsViewPresenter = new SettingsViewPresenter(settingsView, new ProxyInformation());
			settingsViewPresenter.Port = listener.Port;
			settingsViewPresenter.TfsUrl = listener.TfsUrl;
			settingsViewPresenter.IgnoredUsedPort = listener.Port;
			settingsViewPresenter.Show();

			if ((!settingsViewPresenter.Cancelled) &&
			    (SettingsHaveChanged(settingsViewPresenter.Port, settingsViewPresenter.TfsUrl)))
			{
				ApplyNewSettings(settingsViewPresenter.Port, settingsViewPresenter.TfsUrl);
			}
		}

		public void Show()
		{
			view.Show();
		}

		public void StartListener()
		{
			listener.Start();

			view.OnListenerStarted();
		}

		public void StopListener()
		{
			listener.Stop();

			view.OnListenerStopped();
		}

		private void ApplyNewSettings(int port,
		                              string tfsUrl)
		{
			StopListener();

			listener.Port = port;
			listener.TfsUrl = tfsUrl;

			StartListener();
		}

		private bool SettingsHaveChanged(int port,
		                                 string tfsUrl)
		{
			return port != listener.Port || tfsUrl != listener.TfsUrl;
		}

		public void ShowErrors()
		{
			errorsView.Show();			
		}

		public void ViewClosed()
		{
			closed = true;
			errorsView.Close();
		}
	}
}