using System.IO;
using System.Text;
using Attach;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class GetHandlerTests : HandlerTestsBase
    {
        protected GetHandler handler = new GetHandler();

        [Test]
        public void TestHandle()
        {
            ItemMetaData item = new ItemMetaData();
            item.Name = "Foo/Bar.txt";
            Results getItemsResult = stub.Attach(provider.GetItems, item);
            Results readFileResult = stub.Attach(provider.ReadFile, Encoding.Default.GetBytes("asdf"));
            request.Path = "http://localhost:8082/!svn/bc/1234/Foo/Bar.txt";

            handler.Handle(context, tfsUrl);

            string expected = "asdf";
            Assert.AreEqual(expected, Encoding.Default.GetString(((MemoryStream) response.OutputStream).ToArray()));
            Assert.AreEqual("text/plain", response.ContentType);
            Assert.AreEqual(1, getItemsResult.CallCount);
            Assert.AreEqual(1234, getItemsResult.Parameters[0]);
            Assert.AreEqual("/Foo/Bar.txt", getItemsResult.Parameters[1]);
            Assert.AreEqual(1, readFileResult.CallCount);
            Assert.AreEqual(item, readFileResult.Parameters[0]);
        }
    }
}