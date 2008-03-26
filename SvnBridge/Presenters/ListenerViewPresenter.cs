using SvnBridge.Net;
using SvnBridge.PathParsing;
using SvnBridge.Views;

namespace SvnBridge.Presenters
{
	public class ListenerViewPresenter
	{
		private readonly IListenerErrorsView errorsView;
		private readonly IListener listener;
		private string tfsUrl;
		private readonly IListenerView view;
		private bool closed;

		public ListenerViewPresenter(IListenerView view,
		                             IListenerErrorsView errorsView,
		                             IListener listener,
		                             string tfsUrl)
		{
			this.listener = listener;
			this.tfsUrl = tfsUrl;
			this.view = view;
			this.errorsView = errorsView;
			view.Presenter = this;
			errorsView.Presenter = this;

			listener.ListenError += OnListenError;
		}

		public int Port
		{
			get { return listener.Port; }
		}

		public string TfsUrl
		{
			get { return tfsUrl; }
			set { tfsUrl = value; }
		}

		public bool ShouldCloseErrorView
		{
			get { return closed; }
		}

		private void OnListenError(object sender, ListenErrorEventArgs e)
		{
			errorsView.AddError(e.Exception.Message, e.Exception.ToString());
			view.OnListenerError(e.Exception.Message);
		}

		public void ChangeSettings(ISettingsView settingsView)
		{
			var settingsViewPresenter = new SettingsViewPresenter(settingsView, new ProxyInformation());
			settingsViewPresenter.Port = listener.Port;
			settingsViewPresenter.TfsUrl = TfsUrl;
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
			listener.Start(new StaticServerPathParser(TfsUrl));

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
			TfsUrl = tfsUrl;

			StartListener();
		}

		private bool SettingsHaveChanged(int port,
		                                 string tfsUrl)
		{
			return port != listener.Port || tfsUrl != TfsUrl;
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