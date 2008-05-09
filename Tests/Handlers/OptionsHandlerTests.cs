using Attach;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using Xunit;
using SvnBridge.Infrastructure;
using SvnBridge.PathParsing;

namespace SvnBridge.Handlers
{
    public class OptionsHandlerTests : HandlerTestsBase
    {
        protected OptionsHandler handler = new OptionsHandler();

        [Fact]
        public void VerifyHandleDecodesPathWhenInvokingSourceControlProvider()
        {
            Results r = stub.Attach(provider.ItemExists, true);
            request.Path = "http://localhost:8082/Spikes/SvnFacade/trunk/New%20Folder%207";

        	handler.Handle(context, new StaticServerPathParser(tfsUrl, MockRepository.GenerateStub<IProjectInformationRepository>()), null);

            Assert.Equal("/Spikes/SvnFacade/trunk/New Folder 7", r.Parameters[0]);
        }
    }
}
