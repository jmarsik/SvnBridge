using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Interfaces;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using Tests;

namespace SvnBridge.PathParsing
{
    public class StaticServerPathParserTest
    {
        protected MyMocks stubs = new MyMocks();

        [Fact]
        public void VerifyGetLocalPathWhenPathIsRootReturnsRootPath()
        {
            MockRepository mocks = new MockRepository();
            StaticServerPathParser parser = new StaticServerPathParser("http://www.codeplex.com", stubs.CreateObject<ProjectInformationRepository>(null, null));
            IHttpRequest request = mocks.Stub<IHttpRequest>();
            SetupResult.For(request.ApplicationPath ).Return("/");
            mocks.ReplayAll();

            string result = parser.GetLocalPath(request, "http://www.root.com");

            Assert.Equal("/", result);
        }
    }
}
