using Attach;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DeleteTests : WebDavServiceTestsBase
    {
        [Test]
        public void DeleteSvnActActivityId()
        {
            Results r = mock.Attach(provider.DeleteActivity);
            string path = "/!svn/act/5b34ae67-87de-3741-a590-8bda26893532";

            service.Delete(path);

            Assert.AreEqual(1, r.CalledCount);
            Assert.AreEqual("5b34ae67-87de-3741-a590-8bda26893532", r.Parameters[0]);
        }
    }
}