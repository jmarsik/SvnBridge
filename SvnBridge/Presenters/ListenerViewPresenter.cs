using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
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
		private bool? getServerUrlFromRequest;

		public ListenerViewPresenter(IListenerView view,
		                             IListenerErrorsView errorsView,
		                             IListener listener,
		                             string tfsUrl,
									 bool? serverUrlFromRequest)
		{
			getServerUrlFromRequest = serverUrlFromRequest;
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
			SettingsViewPresenter settingsViewPresenter = new SettingsViewPresenter(settingsView, new ProxyInformation());
			settingsViewPresenter.Port = listener.Port;
			settingsViewPresenter.TfsUrl = TfsUrl;
			settingsViewPresenter.GetServerUrlFromRequest = GetServerUrlFromRequest;
			settingsViewPresenter.IgnoredUsedPort = listener.Port;
			settingsViewPresenter.Show();

			if ((!settingsViewPresenter.Cancelled) &&
			    (SettingsHaveChanged(settingsViewPresenter.Port, settingsViewPresenter.TfsUrl,
			                         settingsViewPresenter.GetServerUrlFromRequest)))
			{
				ApplyNewSettings(settingsViewPresenter.Port, settingsViewPresenter.TfsUrl, settingsViewPresenter.GetServerUrlFromRequest);
			}
		}

		public void Show()
		{
			view.Show();
		}

		public void StartListener()
		{
			IPathParser parser;
			
			if (getServerUrlFromRequest == true)
				parser = new RequestBasePathParser(IoC.Resolve<ITfsUrlValidator>());
			else
				parser = new StaticServerPathParser(TfsUrl);

			listener.Start(parser);

			view.OnListenerStarted();
		}

		public void StopListener()
		{
			listener.Stop();

			view.OnListenerStopped();
		}

		private void ApplyNewSettings(int port, string serverUrl, bool? urlFromRequest)
		{
			StopListener();

			listener.Port = port;
			GetServerUrlFromRequest = urlFromRequest;
			TfsUrl = serverUrl;

			StartListener();
		}

		private bool SettingsHaveChanged(int port, string serverUrl, bool? serverUrlFromRequest)
		{
			return port != listener.Port || serverUrl != TfsUrl || serverUrlFromRequest != GetServerUrlFromRequest;
		}

		public bool ? GetServerUrlFromRequest
		{
			get { return getServerUrlFromRequest; }
			set { getServerUrlFromRequest = value; }
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
