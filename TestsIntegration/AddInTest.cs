using System.Net;
using SvnBridge;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using Xunit;

namespace IntegrationTests
{
	public class AddInTest
	{
		[Fact]
		public void CanExtendTypesInContainer_FromExternalAssemblies()
		{
			new BootStrapper().Start();

			Assert.Equal(IPAddress.Loopback, IoC.Resolve<IPAddress>());
		}
	}

	public class TestAddIn : IAssemblyAddin
	{
		#region IAssemblyAddin Members

		public void Initialize(Container container)
		{
			container.OverrideRegisteration<IPAddress>(delegate { return IPAddress.Loopback; });
		}

		#endregion
	}
}