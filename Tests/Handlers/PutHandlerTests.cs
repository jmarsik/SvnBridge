using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Attach;
using SvnBridge.Infrastructure;

namespace SvnBridge.Handlers
{
    [TestFixture]
    public class PutHandlerTests : HandlerTestsBase
    {
        protected PutHandler handler = new PutHandler();

        [Test]
        public void VerifyPathIsDecodedWhenInvokingSourceControlProviderForFolderPath()
        {
            Results r = mock.Attach(provider.WriteFile, false);
            request.Path = "http://localhost:8082//!svn/wrk/be3dd5c3-e77f-f246-a1e8-640012b047a2/Spikes/SvnFacade/trunk/New%20Folder%207/Empty%20File%202.txt";
            request.Input = "SVN\0";

            handler.Handle(context, tfsServerUrl);

            Assert.AreEqual("/Spikes/SvnFacade/trunk/New Folder 7/Empty File 2.txt", r.Parameters[1]);
        }
    }
}
