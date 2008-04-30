using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
    public class StaticServerPathParserTest
    {
        [Fact]
        public void VerifyGetLocalPathWhenPathIsRootReturnsRootPath()
        {
            StaticServerPathParser parser = new StaticServerPathParser("http://www.codeplex.com");
            MockRepository mocks = new MockRepository();
            IHttpRequest request = mocks.Stub<IHttpRequest>();
            SetupResult.For(request.ApplicationPath ).Return("/");
            mocks.ReplayAll();

            string result = parser.GetLocalPath(request, "http://www.root.com");

            Assert.Equal("/", result);
        }
    }
}
