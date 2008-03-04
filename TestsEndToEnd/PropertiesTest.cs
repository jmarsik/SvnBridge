using System;
using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class PropertiesTest : EndToEndTestBase
    {
        [Test]
        public void CanSetAndGetProperty()
        {
            CheckoutAndChangeDirectory();

            Svn("propset myLabel \"WorkItem: %BUGID%\" .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget myLabel");

            Assert.AreEqual("WorkItem: %BUGID%"+Environment.NewLine, actual);
        }

        [Test]
        public void CanSetAndGetSvnIgnore()
        {
            CheckoutAndChangeDirectory();

            Svn("propset svn:ignore *.ing .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget svn:ignore");

            Assert.AreEqual("*.ing", actual.Trim());
        }


        [Test]
        public void CanSetAndGetProperty_WithColon()
        {
            CheckoutAndChangeDirectory();

            Svn("propset bugtraq:label \"WorkItem: %BUGID%\" .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget bugtraq:label");

            Assert.AreEqual("WorkItem: %BUGID%"+Environment.NewLine, actual);
        }
    }
}