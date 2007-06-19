using Attach;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MkActivityTests : WebDavServiceTestsBase
    {
        [Test]
        public void MkActivity()
        {
            Results r = mock.Attach(provider.MakeActivity);
            string path = "/!svn/act/5b34ae67-87de-3741-a590-8bda26893532";

            service.MkActivity(path);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", (string)r.Parameters[0]);
        }
    }
}