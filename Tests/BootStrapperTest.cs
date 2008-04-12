using System;
using System.Collections;
using System.Net;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge
{
	public class BootStrapperTest : IDisposable
	{
		#region Setup/Teardown
        public BootStrapperTest()
        {
            mocks = new MockRepository();
        }

        public void Dispose()
        {
            mocks.VerifyAll();
            IoC.Reset();
        }
		#endregion

		private MockRepository mocks;

		[Fact]
		public void AfterStartingBootStrapper_CanResolveCache_FromContainer()
		{
			new BootStrapper().Start();
			Assert.NotNull(IoC.Resolve<ICache>());
		}

		[Fact]
		public void AfterStartingBootStrapper_WillResolveSameInstanceForPersistentCache()
		{
			new BootStrapper().Start();
			Assert.Same(IoC.Resolve<IPersistentCache>(), IoC.Resolve<IPersistentCache>());
		}

		[Fact]
		public void AfterStartingBootStrapper_CanResolveItems()
		{
			new BootStrapper().Start();
			var dependencies = new Hashtable();
			dependencies["serverUrl"] = "http://codeplex-tfs3:8080/";
			dependencies["projectName"] = "as";
			dependencies["credentials"] = CredentialCache.DefaultCredentials;
			dependencies["rootCachePath"] = ".";
			Assert.NotNull(IoC.Resolve<ITFSSourceControlService>(dependencies));
		}

		[Fact]
		public void ContainerWillCallEnvironmentValidation()
		{
			MockRepository mocks = new MockRepository();

			ICanValidateMyEnvironment validate = mocks.CreateMock<ICanValidateMyEnvironment>();
			validate.ValidateEnvironment();

			mocks.ReplayAll();

			IoC.Container.OverrideRegisteration(typeof(ICanValidateMyEnvironment), validate);


			IoC.Resolve<ICanValidateMyEnvironment>();

			mocks.VerifyAll();
		}

		[Fact]
		public void ContainerWillCallEnvironmentValidation_OnlyOnce()
		{
			MockRepository mocks = new MockRepository();

			ICanValidateMyEnvironment validate = mocks.CreateMock<ICanValidateMyEnvironment>();
			validate.ValidateEnvironment();
			LastCall.Repeat.Once();

			mocks.ReplayAll();

			IoC.Container.OverrideRegisteration(typeof(ICanValidateMyEnvironment), validate);

			IoC.Resolve<ICanValidateMyEnvironment>();
			IoC.Resolve<ICanValidateMyEnvironment>();
			IoC.Resolve<ICanValidateMyEnvironment>();

			mocks.VerifyAll();
		}
    }
}
