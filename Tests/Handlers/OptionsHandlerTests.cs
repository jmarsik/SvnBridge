using Attach;
using NUnit.Framework;
using SvnBridge.Infrastructure;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class OptionsHandlerTests : HandlerTestsBase
    {
        protected OptionsHandler handler = new OptionsHandler();

        [Test]
        public void VerifyHandleDecodesPathWhenInvokingSourceControlProvider()
        {
            Results r = stub.Attach(provider.ItemExists, true);
            request.Path = "http://localhost:8082/Spikes/SvnFacade/trunk/New%20Folder%207";

            handler.Handle(context, tfsUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7", r.Parameters[0]);
        }
    }
}