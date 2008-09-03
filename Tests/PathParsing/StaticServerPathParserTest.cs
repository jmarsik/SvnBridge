using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Interfaces;
using Xunit;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using Tests;
using SvnBridge.Stubs;

namespace SvnBridge.PathParsing
{
    public class StaticServerPathParserTest
    {
        protected MyMocks stubs = new MyMocks();

        [Fact]
        public void VerifyGetLocalPathWhenPathIsRootReturnsRootPath()
        {
            StaticServerPathParser parser = new StaticServerPathParser("http://www.codeplex.com", stubs.CreateObject<ProjectInformationRepository>(null, null));
            StubHttpRequest request = new StubHttpRequest();

            string result = parser.GetLocalPath(request, "http://www.root.com");

            Assert.Equal("/", result);
        }

        [Fact]
        public void Test()
        {
            StaticServerPathParser parser = new StaticServerPathParser("http://svnbridgetesting.redmond.corp.microsoft.com", stubs.CreateObject<ProjectInformationRepository>(null, null));

            string result = parser.GetPathFromDestination("http://svnbridgetesting.redmond.corp.microsoft.com/svn/!svn/wrk/6874f51f-0540-b24f-bbd8-eac3072c5a51/Test/Test2.txt");

            Assert.Equal("/!svn/wrk/6874f51f-0540-b24f-bbd8-eac3072c5a51/Test/Test2.txt", result);
        }
    }
}
