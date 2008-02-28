using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace SvnBridge.Infrastructure
{
    [TestFixture]
    public class CircuitBreakerTest
    {
        [SetUp]
        public void Setup()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Clock.FrozenCurrentTime = new DateTime(2000, 1, 1);
        }

        [TearDown]
        public void Teardown()
        {
            Clock.FrozenCurrentTime = null;
        }

        [Test]
        public void CanCreateProxyForInterface()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            Assert.IsNotNull(foo);
        }

        [Test]
        public void CanCallMethodAndGetValueBack()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            Assert.AreEqual(2, foo.ReturnTwo());
        }

        [Test]
        public void IfProxiedInstanceThrow_ShouldGet_OriginalException_WithStacktrace()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            try
            {
                foo.Throw();
                Assert.Fail("Should throw");
            }
            catch (Exception e)
            {
                Assert.IsTrue(
                    e.ToString().Contains("FooImpl")
                    );
            }
        }

        [Test]
        [ExpectedException(typeof(CircuitTrippedException),
           ExpectedMessage = @"The component SvnBridge.Infrastructure.FooImpl has had more than 10 failures in a 5 minutes period.
The circuit breaker for this component has been tripped and will be in effect until 01/01/2000 00:15:00")]
        public void AfterTenException_InLessThan_FiveMinutes_TripCircuit()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foo.Throw();
                }
                catch
                { }
            }

            foo.Throw();
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "blah")]
        public void WhenCircuitBreakerHasBeenTripped_ItWillResetItself_After_15_Minutes()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foo.Throw();
                }
                catch
                { }
            }

            Clock.FrozenCurrentTime = Clock.Now.AddMinutes(15);
            foo.Throw();
        }

        [Test]
        [ExpectedException(typeof(CircuitTrippedException),
           ExpectedMessage = @"The component SvnBridge.Infrastructure.FooImpl has had more than 10 failures in a 5 minutes period.
The circuit breaker for this component has been tripped and will be in effect until 01/01/2000 00:15:00")]
        public void WhenCircuitBreakerHasTripped_AllMethodOfObject_AreBlocked()
        {
            IFoo foo = CircuitBreaker.For<IFoo, FooImpl>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foo.Throw();
                }
                catch
                { }
            }

            foo.ReturnTwo(); 
        }
    }

    public interface IFoo
    {
        int ReturnTwo();
        void Throw();
    }

    public class FooImpl : IFoo
    {
        public int ReturnTwo()
        {
            return 2;
        }

        public void Throw()
        {
            throw new InvalidOperationException("blah");
        }
    }
}