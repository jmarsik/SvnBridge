using System.Windows.Forms;
using NUnit.Framework;
using SvnBridge.Stubs;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Presenters
{
    [TestFixture]
    public class SettingsViewPresenterTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            stubView = new StubSettingsView();
        }

        #endregion

        private StubSettingsView stubView;

        private SettingsViewPresenter CreatePresenter()
        {
            return new SettingsViewPresenter(stubView);
        }

        [Test]
        public void TestConstructorSetsViewsPresenter()
        {
            SettingsViewPresenter presenter = CreatePresenter();

            Assert.Equal(stubView.Presenter, presenter);
        }

        [Test]
        public void TestShowCallsViewsShow()
        {
            SettingsViewPresenter presenter = CreatePresenter();

            presenter.Show();

            Assert.True(stubView.Show_Called);
        }

        [Test]
        public void TestViewSetsCancelled()
        {
            stubView.Show_Delegate =
                delegate { stubView.DialogResult_Return = DialogResult.Cancel; };
            SettingsViewPresenter presenter = CreatePresenter();

            presenter.Show();

            Assert.True(presenter.Cancelled);
        }

        [Test]
        public void TestViewSetsPort()
        {
            int expected = 8081;
            stubView.Show_Delegate =
                delegate { stubView.Presenter.Port = 8081; };
            SettingsViewPresenter presenter = CreatePresenter();

            presenter.Show();

            Assert.Equal(expected, presenter.Port);
        }

        [Test]
        public void TestViewSetsTfsUrl()
        {
            string expected = "http://foo";
            stubView.Show_Delegate =
                delegate { stubView.Presenter.TfsUrl = "http://foo"; };
            SettingsViewPresenter presenter = CreatePresenter();

            presenter.Show();

            Assert.Equal(expected, presenter.TfsUrl);
        }
    }
}