using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Proxies;

namespace SvnBridge.Infrastructure
{
    public class Container
    {
        private delegate object Creator(IDictionary constructorParams);

        private readonly Dictionary<Type, Creator> typeToCreator = new Dictionary<Type, Creator>();
        private readonly Dictionary<Type, bool> performedValidation = new Dictionary<Type, bool>();

        public void Register(Type service, Type impl)
        {
            if (!typeToCreator.ContainsKey(service))
            {
                lock (typeToCreator)
                {
                    if (!typeToCreator.ContainsKey(service))
                    {
                        List<Type> interceptorTypes = GetInterceptorTypes(impl);
                        Creator creator = GetAutoCreator(service, impl, interceptorTypes);
                        typeToCreator.Add(service, creator);
                    }
                }
            }
        }

        public object Resolve(Type type, IDictionary constructorParams)
        {
            Creator creator;
            bool typeRegistered;
            lock (typeToCreator)
            {
                typeRegistered = typeToCreator.TryGetValue(type, out creator);
            }
            if (!typeRegistered)
            {
                if (type.IsInterface)
                {
                    throw new InvalidOperationException("No component registered for interface " + type);
                }
                Register(type, type);
                lock (typeToCreator)
                {
                    creator = typeToCreator[type];
                }
            }
            object resolve = creator(constructorParams);
            PerformEnvironmentValidation(type, resolve);
            return resolve;
        }

        private Creator GetAutoCreator(Type service, Type impl, List<Type> interceptorTypes)
        {
            return delegate(IDictionary constructorParams)
            {
                object instance = CreateInstance(impl, constructorParams);
                List<IInterceptor> interceptors = new List<IInterceptor>();
                foreach (Type interceptorType in interceptorTypes)
                {
                    interceptors.Add((IInterceptor)Resolve(interceptorType, constructorParams));
                }
                if (interceptors.Count == 0)
                    return instance;
                return ProxyFactory.Create(service, instance, interceptors.ToArray());
            };
        }

        private object CreateInstance(Type type, IDictionary constructorParams)
        {
            List<object> args = new List<object>();
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length != 0)
            {
                foreach (ParameterInfo info in constructors[0].GetParameters())
                {
                    try
                    {
                        object arg;
                        if (constructorParams.Contains(info.Name))
                        {
                            arg = constructorParams[info.Name];
                        }
                        else if (TryGetConfiguration(info.Name) != null)
                    	{
                    		arg = TryGetConfiguration(info.Name);
                    		arg = Convert.ChangeType(arg, info.ParameterType);
                    	}
                    	else
                    	{
                    		arg = Resolve(info.ParameterType, constructorParams);
                    	}
                        args.Add(arg);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(
                            "Failed trying to resolve constructor parameter '" + info.Name + "' for: " + type, e);
                    }
                }
            }
            if (args.Count > 0)
            {
                return Activator.CreateInstance(type, args.ToArray());
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        private object TryGetConfiguration(string name)
        {
            if (ConfigurationManager.AppSettings[name] != null)
            {
                return ConfigurationManager.AppSettings[name];
            }
            if (PerRequest.IsInitialized && PerRequest.Items.Contains(name))
            {
                return PerRequest.Items[name];
            }
            return null;
        }

        private List<Type> GetInterceptorTypes(Type impl)
        {
            List<Type> interceptors = new List<Type>();
            object[] attributes = impl.GetCustomAttributes(typeof(InterceptorAttribute), true);
            foreach (object attribute in attributes)
            {
                Type interceptorType = ((InterceptorAttribute)attribute).Interceptor;
                Register(interceptorType, interceptorType);
                interceptors.Add(interceptorType);
            }
            return interceptors;
        }

        private void PerformEnvironmentValidation(Type type, object resolve)
        {
            ICanValidateMyEnvironment validator = resolve as ICanValidateMyEnvironment;
            if (validator == null)
                return;

            if (!performedValidation.ContainsKey(type))
            {
                lock (performedValidation)
                {
                    if (!performedValidation.ContainsKey(type))
                    {
                        validator.ValidateEnvironment();
                        performedValidation[type] = true;
                    }
                }
            }
        }
    }
}