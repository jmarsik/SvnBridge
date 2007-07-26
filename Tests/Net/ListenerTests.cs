using System;
using NUnit.Framework;
using Assert=CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Net
{
    [TestFixture]
    public class ListenerTests
    {
        [Test]
        public void SetInvalidTfsServerUrlThrows()
        {
            Listener listener = new Listener();        

            Assert.Throws<UriFormatException>(
                delegate
                {
                    listener.TfsServerUrl = "foo";
                });
        }

        [Test]
        public void StartWithoutSettingPortThrows()
        {
            Listener listener = new Listener();
            listener.TfsServerUrl = "http://foo";

            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    listener.Start();
                });
        }

        [Test]
        public void StartWithoutSettingTfsServerUrlThrows()
        {
            Listener listener = new Listener();
            listener.Port = 8081;

            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    listener.Start();
                });
        }

        [Test]
        public void SetPortAfterStartThrows()
        {
            Listener listener = new Listener();
            listener.Port = 8081;
            listener.TfsServerUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    listener.Port = 8082;
                });

            listener.Stop();
        }

        [Test]
        public void SetTfsServerUrlAfterStartThrows()
        {
            Listener listener = new Listener();
            listener.Port = 8081;
            listener.TfsServerUrl = "http://foo";
            listener.Start();

            Assert.Throws<InvalidOperationException>(
                delegate
                {
                    listener.TfsServerUrl = "http://bar";
                });

            listener.Stop();
        }
    }
}