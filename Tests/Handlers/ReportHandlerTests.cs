using System;
using System.Collections.Generic;
using System.Text;
using SvnBridge.Infrastructure;
using Xunit;
using Attach;
using SvnBridge.Net;
using SvnBridge.Handlers;
using SvnBridge.PathParsing;

namespace UnitTests
{
    public class ReportHandlerTests : HandlerTestsBase
    {
        protected ReportHandler handler = new ReportHandler();

        [Fact]
        public void Handle_ErrorOccurs_RequestBodyIsSetInRequestCache()
        {
            stubs.Attach(provider.GetLog, Return.Exception(new Exception("Test")));
            request.Path = "http://localhost:8082/!svn/bc/5532/newFolder4";
            request.Input = "<S:log-report xmlns:S=\"svn:\"><S:start-revision>5532</S:start-revision><S:end-revision>1</S:end-revision><S:limit>100</S:limit><S:discover-changed-paths/><S:path></S:path></S:log-report>";

            Record.Exception(delegate { handler.Handle(context, new PathParserSingleServerWithProjectInPath("http://tfsserver"), null); });

            Assert.NotNull(RequestCache.Items["RequestBody"]);
        }
    }
}
