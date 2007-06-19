using NUnit.Framework;
using SvnBridge;
using SvnBridge.RequestReceiver;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace Tests.SvnBridge
{
    public class ToolTrayPresenterTest
    {
        [TestFixture]
        public class Start
        {
            [Test]
            public void CallsReceiverStart()
            {
                TestableToolTrayPresenter presenter = TestableToolTrayPresenter.Create();

                presenter.Start(8081, "http://foobar");

                Assert.Equal(8081, presenter.Receiver.Start_PortNumber);
                Assert.Equal("http://foobar", presenter.Receiver.Start_TfsServer);
            }

            [Test]
            public void CallsViewOnStart()
            {
                TestableToolTrayPresenter presenter = TestableToolTrayPresenter.Create();

                presenter.Start(8081, "http://foobar");

                Assert.True(presenter.View.OnServerStarted_Called);
            }

            [Test]
            public void SetsPortAndServer()
            {
                TestableToolTrayPresenter presenter = TestableToolTrayPresenter.Create();

                presenter.Start(8081, "http://foobar");

                Assert.Equal(8081, presenter.PortNumber);
                Assert.Equal("http://foobar", presenter.TfsServer);
            }
        }

        [TestFixture]
        public class Stop
        {
            [Test]
            public void CallsReceiverStop()
            {
                TestableToolTrayPresenter presenter = TestableToolTrayPresenter.Create();

                presenter.Stop();

                Assert.True(presenter.Receiver.Stop_Called);
            }

            [Test]
            public void CallsViewOnStop()
            {
                TestableToolTrayPresenter presenter = TestableToolTrayPresenter.Create();

                presenter.Stop();

                Assert.True(presenter.View.OnServerStopped_Called);
            }
        }

        class TestableToolTrayPresenter : ToolTrayPresenter
        {
            public StubToolTrayView View;
            public StubTcpClientRequestReceiver Receiver;

            TestableToolTrayPresenter(StubToolTrayView view,
                                      StubTcpClientRequestReceiver receiver)
                : base(view, receiver)
            {
                View = view;
                Receiver = receiver;
            }

            public static TestableToolTrayPresenter Create()
            {
                return new TestableToolTrayPresenter(new StubToolTrayView(), new StubTcpClientRequestReceiver());
            }
        }

        class StubToolTrayView : IToolTrayView
        {
            public bool OnServerStarted_Called;

            public bool OnServerStopped_Called;

            public void OnServerStarted()
            {
                OnServerStarted_Called = true;
            }

            public void OnServerStopped()
            {
                OnServerStopped_Called = true;
            }
        }
    }

    class StubTcpClientRequestReceiver : ITcpClientRequestReceiver
    {
        public int Start_PortNumber;
        public string Start_TfsServer;

        public bool Stop_Called;

        public void Start(int portNumber,
                          string tfsServer)
        {
            Start_PortNumber = portNumber;
            Start_TfsServer = tfsServer;
        }

        public void Stop()
        {
            Stop_Called = true;
        }
    }
}