using System;
using Xunit;
using SvnBridge.Infrastructure;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Net
{
    public class ListenerTests
    {
        [Fact]
        public void SetInvalidTfsUrlThrows()
        {
            Listener listener = new Listener(new FileLogger());

            Assert.Throws<UriFormatException>(
                delegate { listener.TfsUrl = "foo"; });
        }

        [Fact]
        public void SetPortAfterStartThrows()
        {
            Listener listener = new Listener(new FileLogger());
            listener.Port = 10011;
            listener.TfsUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Port = 8082; });

            listener.Stop();
        }

        [Fact]
        public void SetTfsUrlAfterStartThrows()
        {
            Listener listener = new Listener(new FileLogger());
            listener.Port = 10011;
            listener.TfsUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate { listener.TfsUrl = "http://bar"; });

            listener.Stop();
        }

        [Fact]
        public void StartWithoutSettingPortThrows()
        {
            Listener listener = new Listener(new FileLogger());
            listener.TfsUrl = "http://foo";

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Start(); });
        }

        [Fact]
        public void StartWithoutSettingTfsUrlThrows()
        {
            Listener listener = new Listener(new FileLogger());
            listener.Port = 10011;

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Start(); });
        }
    }
}