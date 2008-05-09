using System;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Stubs;
using Xunit;

namespace SvnBridge.PathParsing
{
    public class RequestBasePathParserTest : IDisposable
    {
        readonly MockRepository mocks = new MockRepository();

        [Fact]
        public void CanParseServerFromUrl()
        {
            ITfsUrlValidator urlValidator = mocks.DynamicMock<ITfsUrlValidator>();
            Expect.Call(urlValidator.IsValidTfsServerUrl("https://tfs03.codeplex.com")).Return(true);
            mocks.ReplayAll();

            RequestBasePathParser parser = new RequestBasePathParser(urlValidator);
            StubHttpRequest request = new StubHttpRequest
            {
                Url = new Uri("http://localhost:8081/tfs03.codeplex.com/SvnBridge")
            };
            string url = parser.GetServerUrl(request, null);
            Assert.Equal("https://tfs03.codeplex.com", url);
        }

        [Fact]
        public void CanParseServerFromUrl_WillUseHttpIfHttpsIsNotValid()
        {
            ITfsUrlValidator urlValidator = mocks.DynamicMock<ITfsUrlValidator>();
            Expect.Call(urlValidator.IsValidTfsServerUrl("https://tfs03.codeplex.com")).Return(false);
            mocks.ReplayAll();

            RequestBasePathParser parser = new RequestBasePathParser(urlValidator);
            IHttpRequest request = new StubHttpRequest
            {
                Url = new Uri("http://localhost:8081/tfs03.codeplex.com/SvnBridge")
            };
            string url = parser.GetServerUrl(request, null);
            Assert.Equal("http://tfs03.codeplex.com", url);
        }


        [Fact]
        public void CanParseServerFromUrl_WithPort()
        {
            ITfsUrlValidator urlValidator = mocks.DynamicMock<ITfsUrlValidator>();
            Expect.Call(urlValidator.IsValidTfsServerUrl("https://tfs03.codeplex.com:8080")).Return(true);
            mocks.ReplayAll();

            RequestBasePathParser parser = new RequestBasePathParser(urlValidator);
            IHttpRequest request = new StubHttpRequest
            {
                Url = new Uri("http://localhost:8081/tfs03.codeplex.com:8080/SvnBridge")
            };
            string url = parser.GetServerUrl(request, null);
            Assert.Equal("https://tfs03.codeplex.com:8080", url);
        }


        [Fact]
        public void CanParseServerFromUrl_WithPortAndNestedFolder()
        {
            ITfsUrlValidator urlValidator = mocks.DynamicMock<ITfsUrlValidator>();
            Expect.Call(urlValidator.IsValidTfsServerUrl("https://tfs03.codeplex.com:8080")).Return(true);
            mocks.ReplayAll();

            RequestBasePathParser parser = new RequestBasePathParser(urlValidator);
            IHttpRequest request = new StubHttpRequest
            {
                Url = new Uri("http://localhost:8081/tfs03.codeplex.com:8080/SvnBridge/Foo")
            };
            string url = parser.GetServerUrl(request,null);
            Assert.Equal("https://tfs03.codeplex.com:8080", url);
        }


        [Fact]
        public void CanGetLocalPath_WithoutServerUrl()
        {
            IHttpRequest request = mocks.Stub<IHttpRequest>();
            SetupResult.For(request.Url).Return(new Uri("http://localhost:8081/tfs03.codeplex.com:8080/SvnBridge"));

            ITfsUrlValidator urlValidator = mocks.Stub<ITfsUrlValidator>();
            mocks.ReplayAll();

            RequestBasePathParser parser = new RequestBasePathParser(urlValidator);
            string url = parser.GetLocalPath(request);
            Assert.Equal("/SvnBridge", url);

        }

        #region IDisposable Members

        public void Dispose()
        {
            mocks.VerifyAll();
        }

        #endregion
    }
}