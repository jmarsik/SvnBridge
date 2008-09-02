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
            StubCanValidateMyEnvironment result = container.Resolve<StubCanValidateMyEnvironment>();

            Assert.NotNull(result);
        }

		[Fact]
		public void Resolve_UsingInterfaceAfterRegistering_ReturnsRegisteredConcreteType()
		{
            container.RegisterType(typeof(ICanValidateMyEnvironment), typeof(StubCanValidateMyEnvironment));

            StubCanValidateMyEnvironment result = container.Resolve<ICanValidateMyEnvironment>() as StubCanValidateMyEnvironment;

            Assert.NotNull(result);
		}

        [Fact]
        public void Resolve_UsingInterfaceWithoutRegistering_ReturnsError()
        {
            Exception result = Record.Exception(delegate() { container.Resolve<ICanValidateMyEnvironment>(); });

            Assert.NotNull(result);
            Assert.IsAssignableFrom<InvalidOperationException>(result);
        }

        [Fact]
        public void Resolve_ConstructorParametersAreSpecified_ConstructorCalledWithCorrectValues()
        {
            Hashtable constructorParams = new Hashtable();
            constructorParams["containerTest"] = 987;

            StubContainerTest result = container.Resolve<StubContainerTest>(constructorParams);

            Assert.Equal(987, result.Constructor_Param);
        }

        [Fact]
        public void Resolve_ConstructorParametersAreSpecifiedInConfig_ConstructorCalledWithValuesFromConfig()
        {
            StubContainerTest result = container.Resolve<StubContainerTest>();

            Assert.Equal(123, result.Constructor_Param);
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

        [Fact]
        public void Resolve_ClassImplementsICanValidateMyEnvironment_ContainerWillCallEnvironmentValidation()
        {
            container.RegisterType(typeof(ICanValidateMyEnvironment), typeof(StubCanValidateMyEnvironment));

            StubCanValidateMyEnvironment result = (StubCanValidateMyEnvironment)container.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, result.ValidateEnvironment_CallCount);
        }

        [Fact]
        public void Resolve_ClassImplementsICanValidateMyEnvironment_ContainerWillCallEnvironmentValidationOnlyOnce()
        {
            container.RegisterType(typeof(ICanValidateMyEnvironment), typeof(StubCanValidateMyEnvironment));

            StubCanValidateMyEnvironment result1 = (StubCanValidateMyEnvironment)container.Resolve<ICanValidateMyEnvironment>();
            StubCanValidateMyEnvironment result2 = (StubCanValidateMyEnvironment)container.Resolve<ICanValidateMyEnvironment>();

            Assert.Equal(1, result1.ValidateEnvironment_CallCount);
            Assert.Equal(0, result2.ValidateEnvironment_CallCount);
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

        public class StubContainerTest2
        {
            public StubContainerTest Constructor_Param;

            public StubContainerTest2(StubContainerTest param)
            {
                Constructor_Param = param;
            }
        }

        [Interceptor(typeof(TestInterceptor))]
        public class StubContainerTest : MarshalByRefObject
        {
            public int Constructor_Param;

            public StubContainerTest(int containerTest)
            {
                Constructor_Param = containerTest;
            }

            public void Test()
            {
            }
        }
    }
}
