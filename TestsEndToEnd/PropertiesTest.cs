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

        [Test]
        public void CanWriteAndReadBugTrackingProperties()
        {
            CheckoutAndChangeDirectory();

            Svn("propset bugtraq:label \"Work Item:\" .");
            Svn("propset bugtraq:message \"Work Item: %BUGID%\" .");
            Svn("propset bugtraq:number true .");
            Svn("propset bugtraq:url http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=%BUGID% .");
            Svn("propset bugtraq:warnifnoissue true .");

            Svn("commit -m \"bug tracking props\"");

            CheckoutAgainAndChangeDirectory();

            string svn = Svn("propget bugtraq:label");
            Assert.AreEqual("Work Item:", svn.Trim());

            svn = Svn("propget bugtraq:message");
            Assert.AreEqual("Work Item: %BUGID%", svn.Trim());

            svn = Svn("propget bugtraq:number");
            Assert.AreEqual("true", svn.Trim());

            svn = Svn("propget bugtraq:url");
            Assert.AreEqual("http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=%BUGID%", svn.Trim());

            svn = Svn("propget bugtraq:warnifnoissue");
            Assert.AreEqual("true", svn.Trim());
        }
    }
}