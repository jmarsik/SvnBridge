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
    }
}
