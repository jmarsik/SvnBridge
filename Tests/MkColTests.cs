using Attach;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MkColTests : WebDavServiceTestsBase
    {
        [Test]
        public void MkColSvnWrkActivityIdDirectory()
        {
            Results r = mock.Attach(provider.MakeCollection);
            string path = "//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty";
            string host = "localhost:8081";

            string response = service.MkCol(path, host);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", (string)r.Parameters[0]);
            Assert.AreEqual("/Spikes/SvnFacade/trunk/Empty", (string)r.Parameters[1]);
            Assert.AreEqual("http://localhost:8081//!svn/wrk/5b34ae67-87de-3741-a590-8bda26893532/Spikes/SvnFacade/trunk/Empty", response);
        }
    }
}