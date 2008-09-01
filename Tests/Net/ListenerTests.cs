using System;
using Rhino.Mocks;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.NullImpl;
using SvnBridge.PathParsing;
using Xunit;
using Assert=CodePlex.NUnitExtensions.Assert;
using SvnBridge.Infrastructure;
using Tests;

namespace SvnBridge.Net
{
    public class ListenerTests
    {
        protected MyMocks stub = new MyMocks();

        [Fact]
        public void SetPortAfterStartThrows()
        {
            Listener listener = new Listener(stub.CreateObject<DefaultLogger>(), MockRepository.GenerateStub<IActionTracking>());
            listener.Port = 10011;
            listener.Start(new StaticServerPathParser("http://foo", MockRepository.GenerateStub<IProjectInformationRepository>()));

            Assert.Throws<InvalidOperationException>(
                delegate { listener.Port = 8082; });

            listener.Stop();
        }

        [Fact]
        public void StartWithoutSettingPortThrows()
        {
            Listener listener = new Listener(stub.CreateObject<DefaultLogger>(), MockRepository.GenerateStub<IActionTracking>());
            
            Assert.Throws<InvalidOperationException>(
				delegate { listener.Start(new StaticServerPathParser("http://foo", MockRepository.GenerateStub<IProjectInformationRepository>())); });
        }
    }
}