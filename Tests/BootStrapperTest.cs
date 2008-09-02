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
		public void ContainerWillCallEnvironmentValidation()
		{
			StubCanValidateMyEnvironment validate = new StubCanValidateMyEnvironment();
			IoC.Container.OverrideRegisteration(typeof(ICanValidateMyEnvironment), validate);

			IoC.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, validate.ValidateEnvironment_CallCount);
		}

		[Fact]
		public void ContainerWillCallEnvironmentValidation_OnlyOnce()
		{
            StubCanValidateMyEnvironment validate = new StubCanValidateMyEnvironment();
			IoC.Container.OverrideRegisteration(typeof(ICanValidateMyEnvironment), validate);

			IoC.Resolve<ICanValidateMyEnvironment>();
			IoC.Resolve<ICanValidateMyEnvironment>();
			IoC.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, validate.ValidateEnvironment_CallCount);
        }
    }
}
