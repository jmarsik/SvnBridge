using System;
using SvnBridge.NullImpl;
using SvnBridge.PathParsing;
using Xunit;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Net
{
    public class ListenerTests
    {
        [Fact]
        public void SetPortAfterStartThrows()
        {
            Listener listener = new Listener(new NullLogger());
            listener.Port = 10011;
            listener.Start(new StaticServerPathParser("http://foo"));

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Port = 8082; });

            listener.Stop();
        }

        [Fact]
        public void StartWithoutSettingPortThrows()
        {
			Listener listener = new Listener(new NullLogger());
            
            Assert.Throws<InvalidOperationException>(
				delegate { listener.Start(new StaticServerPathParser("http://foo")); });
        }
    }
}