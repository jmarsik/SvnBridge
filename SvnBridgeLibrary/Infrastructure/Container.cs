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
        private static Container container = new Container();

        public static void Reset()
        {
            container = new Container();
        }

        public static void Register(Type service, Type impl)
        {
            container.RegisterType(service, impl);
        }

        public static void Register(Type service, object impl)
        {
            container.RegisterType(service, impl);
        }

        public static T Resolve<T>()
        {
            return (T)container.ResolveType(typeof(T), new Hashtable());
        }

        public static T Resolve<T>(IDictionary constructorParams)
        {
            return (T)container.ResolveType(typeof(T), constructorParams);
        }

        private delegate object Creator(IDictionary constructorParams);

        private readonly Dictionary<Type, Creator> typeToCreator = new Dictionary<Type, Creator>();
        private readonly Dictionary<Type, bool> performedValidation = new Dictionary<Type, bool>();

        public void RegisterType(Type service, Type impl)
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

        public void RegisterType(Type service, object impl)
        {
            typeToCreator.Add(service, delegate(IDictionary constructorParams) { return impl; });
        }

        public object ResolveType(Type type, IDictionary constructorParams)
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
                RegisterType(type, type);
                lock (typeToCreator)
                {
                    creator = typeToCreator[type];
                }
            }
            return creator(constructorParams);
        }

        private Creator GetAutoCreator(Type service, Type impl, List<Type> interceptorTypes)
        {
            return delegate(IDictionary constructorParams)
            {
                object instance = CreateInstance(impl, constructorParams);
                List<IInterceptor> interceptors = new List<IInterceptor>();
                foreach (Type interceptorType in interceptorTypes)
                {
                    interceptors.Add((IInterceptor)ResolveType(interceptorType, constructorParams));
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
                    		arg = ResolveType(info.ParameterType, constructorParams);
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
            if (Configuration.AppSettings(name) != null)
            {
                return Configuration.AppSettings(name);
            }
            if (RequestCache.IsInitialized && RequestCache.Items.Contains(name))
            {
                return RequestCache.Items[name];
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
                RegisterType(interceptorType, interceptorType);
                interceptors.Add(interceptorType);
            }
            return interceptors;
        }
    }
}