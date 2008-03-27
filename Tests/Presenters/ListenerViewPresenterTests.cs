using System.Windows.Forms;
using Xunit;
using SvnBridge.Stubs;
using Assert = CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Presenters
{
	public class ListenerViewPresenterTests
	{
		#region Setup/Teardown

        public ListenerViewPresenterTests()
        {
            stubListener = new StubListener();
            stubView = new StubListenerView();
            stubErrorView = new StubErrorsView();
        }

		#endregion

		private readonly StubListenerView stubView;
		private readonly StubListener stubListener;
		private readonly StubErrorsView stubErrorView;

		private ListenerViewPresenter CreatePresenter()
		{
			return new ListenerViewPresenter(stubView, stubErrorView, stubListener, "http://foo", null);
		}

		[Fact]
		public void TestCancelChangeSettingsDoesntStopListener()
		{
			stubListener.Get_Port = 8081;
			stubListener.Get_TfsUrl = "http://foo";
			ListenerViewPresenter presenter = CreatePresenter();
			StubSettingsView stubSettingsView = new StubSettingsView();
			stubSettingsView.Show_Delegate =
				delegate
				{
					stubSettingsView.Presenter.Port = 8082;
					stubSettingsView.Presenter.TfsUrl = "http://foo";
					stubSettingsView.DialogResult_Return = DialogResult.Cancel;
				};

			presenter.ChangeSettings(stubSettingsView);

			Assert.False(stubListener.Stop_Called);
		}

		[Fact]
		public void TestChangeSettingsDefaultsToExistingSettings()
		{
			stubListener.Get_Port = 8081;
			stubListener.Get_TfsUrl = "http://foo";
			ListenerViewPresenter presenter = CreatePresenter();
			StubSettingsView stubSettingsView = new StubSettingsView();

			presenter.ChangeSettings(stubSettingsView);

			Assert.Equal(stubSettingsView.Presenter.Port, 8081);
			Assert.Equal(stubSettingsView.Presenter.TfsUrl, "http://foo");
		}

		[Fact]
		public void TestChangeSettingsWithChangesStopsAndStartsListener()
		{
			stubListener.Get_Port = 8081;
			stubListener.Get_TfsUrl = "http://foo";
			ListenerViewPresenter presenter = CreatePresenter();
			StubSettingsView stubSettingsView = new StubSettingsView();
			stubSettingsView.Show_Delegate =
				delegate
				{
					stubSettingsView.Presenter.Port = 8082;
					stubSettingsView.Presenter.TfsUrl = "http://foo";
				};

			presenter.ChangeSettings(stubSettingsView);

			Assert.True(stubListener.Stop_Called);
			Assert.True(stubListener.Start_Called);
		}

		[Fact]
		public void TestChangeSettingsWithNoChangesDoesntStopListener()
		{
			stubListener.Get_Port = 8081;
			stubListener.Get_TfsUrl = "http://foo";
			ListenerViewPresenter presenter = CreatePresenter();
			StubSettingsView stubSettingsView = new StubSettingsView();
			stubSettingsView.Show_Delegate =
				delegate
				{
					stubSettingsView.Presenter.Port = 8081;
					stubSettingsView.Presenter.TfsUrl = "http://foo";
				};

			presenter.ChangeSettings(stubSettingsView);

			Assert.False(stubListener.Stop_Called);
		}

		[Fact]
		public void TestConstructorSetsViewsPresenter()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			Assert.Equal(presenter, stubView.Set_Presenter);
		}

		[Fact]
		public void TestGetPortReturnsListenersPort()
		{
			int expected = 8081;
			ListenerViewPresenter presenter = CreatePresenter();

			stubListener.Get_Port = 8081;

			Assert.Equal(expected, presenter.Port);
		}

		[Fact]
		public void TestGetTfsUrlReturnsListenersTfsUrl()
		{
			string expected = "http://foo";
			ListenerViewPresenter presenter = CreatePresenter();

			stubListener.Get_TfsUrl = "http://foo";

			Assert.Equal(expected, presenter.TfsUrl);
		}

		[Fact]
		public void WhenListenerRaiseAnErrorWillShowInView()
		{
			CreatePresenter();
			stubListener.RaiseListenErrorEvent("blah");
			Assert.True(stubView.OnListenerError_Called);
		}

		[Fact]
		public void WhenListenerRaiseAnErrorWillAddToErrorsView()
		{
			CreatePresenter();
			stubListener.RaiseListenErrorEvent("blah");
			Assert.True(stubErrorView.AddError_Called);
		}

		[Fact]
		public void WhenAskedToShowErrorsWillShowErrorsView()
		{
			CreatePresenter().ShowErrors();
			Assert.True(stubErrorView.Show_Called);
		}

		[Fact]
		public void TestShowCallsViewsShow()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			presenter.Show();

			Assert.True(stubView.Show_Called);
		}

		[Fact]
		public void TestStartListenerCallsViewsOnListenerStarted()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			presenter.StartListener();

			Assert.True(stubView.OnListenerStarted_Called);
		}

		[Fact]
		public void TestStartListenerStartListener()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			presenter.StartListener();

			Assert.True(stubListener.Start_Called);
		}

		[Fact]
		public void TestStopListenerCallsViewsOnListenerStopped()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			presenter.StopListener();

			Assert.True(stubView.OnListenerStopped_Called);
		}

		[Fact]
		public void TestStopListenerStopsListener()
		{
			ListenerViewPresenter presenter = CreatePresenter();

			presenter.StopListener();

			Assert.True(stubListener.Stop_Called);
		}
	}
}
