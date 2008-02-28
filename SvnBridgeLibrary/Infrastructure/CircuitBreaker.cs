using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;

namespace SvnBridge.Infrastructure
{
    public static class CircuitBreaker
    {
        public static TInterface For<TInterface, TService>()
            where TService : TInterface, new()
        {
            return
                (TInterface) new CircuitBreakerRemotingProxy(typeof (TInterface), new TService()).GetTransparentProxy();
        }

        #region Nested type: CircuitBreakerRemotingProxy

        public class CircuitBreakerRemotingProxy : RealProxy
        {
            private readonly TimeSpan circuitBreakerTrippedTime = TimeSpan.FromMinutes(15);
            private readonly List<FailureInformation> failures = new List<FailureInformation>();
            private readonly int maxFailuresCount = 10;
            private readonly TimeSpan retainAgeForFailures = TimeSpan.FromMinutes(5);
            private readonly object target;
            private DateTime? breakerExpiry;

            public CircuitBreakerRemotingProxy(Type classToProxy,
                                               object target)
                : base(classToProxy)
            {
                this.target = target;
            }


            public override IMessage Invoke(IMessage msg)
            {
                AssertCircuitIsNotTripped();

                IMethodCallMessage callMessage = msg as IMethodCallMessage;

                if (callMessage == null)
                {
                    return null;
                }

                try
                {
                    object result = callMessage.MethodBase.Invoke(target, callMessage.Args);
                    return new MethodReturnMessageWrapper(
                        new ReturnMessage(result, new object[0], 0, callMessage.LogicalCallContext, callMessage)
                        );
                }
                catch (TargetInvocationException e)
                {
                    Exception exception = e.InnerException;

                    lock (failures)
                    {
                        failures.Add(new FailureInformation(exception));
                    }

                    typeof (Exception).GetMethod("InternalPreserveStackTrace",
                                                 BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(exception, new object[0]);

                    throw exception;
                }
            }

            private void AssertCircuitIsNotTripped()
            {
                if (breakerExpiry > Clock.Now)
                {
                    throw new CircuitTrippedException(BuildErrorMessage());
                }
                lock (failures)
                {
                    ClearOldFailures();
                    if (failures.Count >= maxFailuresCount)
                    {
                        breakerExpiry = Clock.Now.Add(circuitBreakerTrippedTime);
                        throw new CircuitTrippedException(BuildErrorMessage());
                    }
                }
            }

            private string BuildErrorMessage()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("The component ")
                    .Append(target)
                    .Append(" has had more than ")
                    .Append(maxFailuresCount)
                    .Append(" failures in a ")
                    .Append(retainAgeForFailures.TotalMinutes)
                    .AppendLine(" minutes period.")
                    .Append("The circuit breaker for this component has been tripped and will be in effect until ")
                    .Append(breakerExpiry);

                return sb.ToString();
            }

            private void ClearOldFailures()
            {
                List<FailureInformation> toRemove = new List<FailureInformation>();
                foreach (FailureInformation failure in failures)
                {
                    if ((Clock.Now - failure.At) > retainAgeForFailures)
                    {
                        toRemove.Add(failure);
                    }
                }
                foreach (FailureInformation information in toRemove)
                {
                    failures.Remove(information);
                }
            }

            #region Nested type: FailureInformation

            public class FailureInformation
            {
                public readonly DateTime At = Clock.Now;
                public Exception Exception;

                public FailureInformation(Exception exception)
                {
                    Exception = exception;
                }
            }

            #endregion
        }

        #endregion
    }
}