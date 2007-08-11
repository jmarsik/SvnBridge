using NUnit.Framework;
using SvnBridge.Stubs;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Presenters
{
    [TestFixture]
    public class ListenerViewPresenterTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            stubListener = new StubListener();
            stubView = new StubListenerView();
        }

        #endregion

        private StubListenerView stubView;
        private StubListener stubListener;

        private ListenerViewPresenter CreatePresenter()
        {
            return new ListenerViewPresenter(stubView, stubListener);
        }

        [Test]
        public void TestConstructorSetsViewsPresenter()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            Assert.Equal(presenter, stubView.Set_Presenter);
        }

        [Test]
        public void TestGetPortReturnsListenersPort()
        {
            int expected = 8081;
            ListenerViewPresenter presenter = CreatePresenter();

            stubListener.Get_Port = 8081;

            Assert.Equal(expected, presenter.Port);
        }

        [Test]
        public void TestGetTfsUrlReturnsListenersTfsUrl()
        {
            string expected = "http://foo";
            ListenerViewPresenter presenter = CreatePresenter();

            stubListener.Get_TfsUrl = "http://foo";

            Assert.Equal(expected, presenter.TfsUrl);
        }

        [Test]
        public void TestShowCallsViewsShow()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            presenter.Show();

            Assert.True(stubView.Show_Called);
        }

        [Test]
        public void TestStartListenerCallsViewsOnListenerStarted()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            presenter.StartListener();

            Assert.True(stubView.OnListenerStarted_Called);
        }

        [Test]
        public void TestStartListenerStartListener()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            presenter.StartListener();

            Assert.True(stubListener.Start_Called);
        }

        [Test]
        public void TestStopListenerCallsViewsOnListenerStopped()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            presenter.StopListener();

            Assert.True(stubView.OnListenerStopped_Called);
        }

        [Test]
        public void TestStopListenerStopsListener()
        {
            ListenerViewPresenter presenter = CreatePresenter();

            presenter.StopListener();

            Assert.True(stubListener.Stop_Called);
        }

        [Test]
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

        [Test]
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

        [Test]
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
                    stubSettingsView.Presenter.Cancelled = true;
                };

            presenter.ChangeSettings(stubSettingsView);

            Assert.False(stubListener.Stop_Called);
        }
    }
}