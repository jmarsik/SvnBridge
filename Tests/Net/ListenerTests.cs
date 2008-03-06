using System;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Net
{
    [TestFixture]
    public class ListenerTests
    {
        [Test]
        public void SetInvalidTfsUrlThrows()
        {
            Listener listener = new Listener(new ConsoleLogger());

            Assert.Throws<UriFormatException>(
                delegate { listener.TfsUrl = "foo"; });
        }

        [Test]
        public void SetPortAfterStartThrows()
        {
            Listener listener = new Listener(new ConsoleLogger());
            listener.Port = 10011;
            listener.TfsUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Port = 8082; });

            listener.Stop();
        }

        [Test]
        public void SetTfsUrlAfterStartThrows()
        {
            Listener listener = new Listener(new ConsoleLogger());
            listener.Port = 10011;
            listener.TfsUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate { listener.TfsUrl = "http://bar"; });

            listener.Stop();
        }

        [Test]
        public void StartWithoutSettingPortThrows()
        {
            Listener listener = new Listener(new ConsoleLogger());
            listener.TfsUrl = "http://foo";

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Start(); });
        }

        [Test]
        public void StartWithoutSettingTfsUrlThrows()
        {
            Listener listener = new Listener(new ConsoleLogger());
            listener.Port = 10011;

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Start(); });
        }
    }
}