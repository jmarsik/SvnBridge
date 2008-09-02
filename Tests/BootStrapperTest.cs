using System;
using System.Collections;
using System.Net;
using Xunit;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;
using SvnBridge.Cache;
using Tests;

namespace SvnBridge
{
	public class BootStrapperTest : IDisposable
	{
        public void Dispose()
        {
            IoC.Reset();
        }

		[Fact]
		public void AfterStartingBootStrapper_CanResolveCache_FromContainer()
		{
			new BootStrapper().Start();
			Assert.NotNull(IoC.Resolve<WebCache>());
		}

		[Fact]
		public void AfterStartingBootStrapper_CanResolveItems()
		{
			new BootStrapper().Start();
            Hashtable dependencies = new Hashtable();
			dependencies["serverUrl"] = "http://codeplex-tfs3:8080/";
			dependencies["projectName"] = "as";
			dependencies["credentials"] = CredentialCache.DefaultCredentials;
			dependencies["rootCachePath"] = ".";
			Assert.NotNull(IoC.Resolve<TFSSourceControlService>(dependencies));
		}

        [Fact]
        public void Resolve_TypeNotRegisteredButIsConcrete_AutomaticallyRegisterType()
        {
            StubCanValidateMyEnvironment result = (StubCanValidateMyEnvironment)IoC.Resolve<StubCanValidateMyEnvironment>();

            Assert.Equal(1, result.ValidateEnvironment_CallCount);
        }

		[Fact]
		public void ContainerWillCallEnvironmentValidation()
		{
            IoC.Register(typeof(ICanValidateMyEnvironment), typeof(StubCanValidateMyEnvironment));

            StubCanValidateMyEnvironment result = (StubCanValidateMyEnvironment)IoC.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, result.ValidateEnvironment_CallCount);
		}

		[Fact]
		public void ContainerWillCallEnvironmentValidation_OnlyOnce()
		{
            IoC.Register(typeof(ICanValidateMyEnvironment), typeof(StubCanValidateMyEnvironment));

            StubCanValidateMyEnvironment result1 = (StubCanValidateMyEnvironment)IoC.Resolve<ICanValidateMyEnvironment>();
            StubCanValidateMyEnvironment result2 = (StubCanValidateMyEnvironment)IoC.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, result1.ValidateEnvironment_CallCount);
            Assert.Equal(0, result2.ValidateEnvironment_CallCount);
        }
    }
}
