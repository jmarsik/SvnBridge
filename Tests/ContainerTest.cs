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
	public class ContainerTest : IDisposable
	{
        private TestableContainer container;

        public ContainerTest()
        {
            container = new TestableContainer();
        }

        public void Dispose()
        {
            TestInterceptor.Invoke_WasCalled = false;
        }

        public class TestableContainer : Container
        {
            public new T Resolve<T>()
            {
                return (T)ResolveType(typeof(T), new Hashtable());
            }

            public new T Resolve<T>(IDictionary constructorParams)
            {
                return (T)ResolveType(typeof(T), constructorParams);
            }
        }

        [Fact]
        public void Resolve_UsingConcreteTypeWithoutRegistering_SuccessfullyReturnsObject()
        {
            StubContainerTest result = container.Resolve<StubContainerTest>();

            Assert.NotNull(result);
        }

		[Fact]
		public void Resolve_UsingInterfaceAfterRegistering_ReturnsRegisteredConcreteType()
		{
            container.RegisterType(typeof(IStubContainerTest), typeof(StubContainerTest2));

            StubContainerTest2 result = container.Resolve<IStubContainerTest>() as StubContainerTest2;

            Assert.NotNull(result);
		}

        [Fact]
        public void Resolve_UsingInterfaceWithoutRegistering_ReturnsError()
        {
            Exception result = Record.Exception(delegate() { container.Resolve<IStubContainerTest>(); });

            Assert.NotNull(result);
            Assert.IsAssignableFrom<InvalidOperationException>(result);
        }

        [Fact]
        public void Resolve_ConstructorParametersAreSpecified_ConstructorCalledWithCorrectValues()
        {
            Hashtable constructorParams = new Hashtable();
            constructorParams["cacheEnabled"] = false;

            StubContainerTest result = container.Resolve<StubContainerTest>(constructorParams);

            Assert.Equal(false, result.Constructor_Param);
        }

        [Fact]
        public void Resolve_ConstructorParametersAreSpecifiedInConfig_ConstructorCalledWithValuesFromConfig()
        {
            StubContainerTest result = container.Resolve<StubContainerTest>();

            Assert.Equal(true, result.Constructor_Param);
        }

        [Fact]
        public void Resolve_ConstructorParametersIncludeType_ContainerCreatesTypeForConstructor()
        {
            StubContainerTest2 result = container.Resolve<StubContainerTest2>();

            Assert.NotNull(result.Constructor_Param);
        }

        [Fact]
        public void Resolve_ClassHasInterceptorSpecified_InvokingMethodCallsInterceptor()
        {
            StubContainerTest result = container.Resolve<StubContainerTest>();

            result.Test();

            Assert.True(TestInterceptor.Invoke_WasCalled);
        }

        public class TestInterceptor : IInterceptor
        {
            public static bool Invoke_WasCalled;

            public void Invoke(IInvocation invocation)
            {
                Invoke_WasCalled = true;
                invocation.Proceed();
            }
        }

        public class StubContainerTest2 : IStubContainerTest
        {
            public StubContainerTest Constructor_Param;

            public StubContainerTest2(StubContainerTest param)
            {
                Constructor_Param = param;
            }
        }

        public interface IStubContainerTest
        {
        }

        [Interceptor(typeof(TestInterceptor))]
        public class StubContainerTest : MarshalByRefObject
        {
            public bool Constructor_Param;

            public StubContainerTest(bool cacheEnabled)
            {
                Constructor_Param = cacheEnabled;
            }

            public void Test()
            {
            }
        }
    }
}
