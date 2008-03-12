using System;
using Xunit;

namespace TestsEndToEnd
{
    public class PropertiesTest : EndToEndTestBase
    {
        [Fact]
        public void CanSetAndGetProperty()
        {
            CheckoutAndChangeDirectory();

            Svn("propset myLabel \"WorkItem: %BUGID%\" .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget myLabel");

            Assert.Equal("WorkItem: %BUGID%"+Environment.NewLine, actual);
        }

        [Fact]
        public void CanSetAndGetSvnIgnore()
        {
            CheckoutAndChangeDirectory();

            Svn("propset svn:ignore *.ing .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget svn:ignore");

            Assert.Equal("*.ing", actual.Trim());
        }


        [Fact]
        public void CanSetAndGetProperty_WithColon()
        {
            CheckoutAndChangeDirectory();

            Svn("propset bugtraq:label \"WorkItem: %BUGID%\" .");

            Svn("commit -m propset");

            CheckoutAgainAndChangeDirectory();

            string actual = Svn("propget bugtraq:label");

            Assert.Equal("WorkItem: %BUGID%"+Environment.NewLine, actual);
        }

        [Fact]
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
            Assert.Equal("Work Item:", svn.Trim());

            svn = Svn("propget bugtraq:message");
            Assert.Equal("Work Item: %BUGID%", svn.Trim());

            svn = Svn("propget bugtraq:number");
            Assert.Equal("true", svn.Trim());

            svn = Svn("propget bugtraq:url");
            Assert.Equal("http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=%BUGID%", svn.Trim());

            svn = Svn("propget bugtraq:warnifnoissue");
            Assert.Equal("true", svn.Trim());
        }
    }
}