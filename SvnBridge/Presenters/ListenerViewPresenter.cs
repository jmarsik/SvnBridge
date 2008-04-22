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
		private readonly IListenerView view;
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

		public int Port
		{
			get { return listener.Port; }
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
			settingsViewPresenter.IgnoredUsedPort = listener.Port;
			settingsViewPresenter.Show();

			if ((!settingsViewPresenter.Cancelled) && (SettingsHaveChanged(settingsViewPresenter.Port)))
			{
				ApplyNewSettings(settingsViewPresenter.Port);
			}
		}

		public void Show()
		{
			view.Show();
		}

		public void StartListener()
		{
			IPathParser parser = new RequestBasePathParser(IoC.Resolve<ITfsUrlValidator>());
			listener.Start(parser);
			view.OnListenerStarted();
		}

		public void StopListener()
		{
			listener.Stop();
			view.OnListenerStopped();
		}

		private void ApplyNewSettings(int port)
		{
			StopListener();
			listener.Port = port;
			StartListener();
		}

		private bool SettingsHaveChanged(int port)
		{
			return port != listener.Port;
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
