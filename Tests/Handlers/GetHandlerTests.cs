using System.IO;
using System.Text;
using Attach;
using SvnBridge.Interfaces;
using Xunit;
using SvnBridge.Infrastructure;
using SvnBridge.PathParsing;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class GetHandlerTests : HandlerTestsBase
    {
        protected GetHandler handler = new GetHandler();

        [Fact]
        public void TestHandle()
        {
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            Results getItemsResult = stubs.Attach(provider.GetItems, item);
            Results readFileResult = stubs.AttachReadFile(provider.ReadFile, Encoding.Default.GetBytes("asdf"));
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo/Bar.txt";

        	handler.Handle(context, new PathParserProjectInPath(tfsUrl, stubs.CreateObject<ProjectInformationRepository>(null, null)), null);

            string expected = "asdf";
            Assert.Equal(expected, Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray()));
            Assert.Equal("text/plain", response.ContentType);
            Assert.Equal(1, getItemsResult.CallCount);
            Assert.Equal(1234, getItemsResult.Parameters[0]);
            Assert.Equal("/Foo/Bar.txt", getItemsResult.Parameters[1]);
            Assert.Equal(1, readFileResult.CallCount);
            Assert.Equal(item, readFileResult.Parameters[0]);
        }
    }
}
