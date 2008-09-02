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
        public delegate object Creator(Container c, IDictionary dependencies);

        private readonly Dictionary<string, object> configuration = new Dictionary<string, object>();
        private readonly Dictionary<Type, bool> performedValidation = new Dictionary<Type, bool>();
        private readonly Dictionary<Type, Creator> typeToCreator = new Dictionary<Type, Creator>();

    	public Dictionary<string, object> Configuration
        {
            get { return configuration; }
        }

        public void Register(Type service, Type impl, params Type[] interceptorsType)
        {
            if (!typeToCreator.ContainsKey(service))
            {
                foreach (Type interceptorType in interceptorsType)
                {
                    Register(interceptorType, interceptorType);
                }
                Creator creator = GetAutoCreator(service, impl, interceptorsType);
                typeToCreator.Add(service, creator);
            }
        }

        public object Resolve(Type type, IDictionary dependencies)
        {
            Creator creator;
            if (typeToCreator.TryGetValue(type, out creator) == false)
            {
                throw new InvalidOperationException("No component registered for " + type);
            }
            object resolve = creator(this, dependencies);
            PerformEnvironmentValidation(type, resolve);
            return resolve;
        }

        private Creator GetAutoCreator(Type service, Type impl, params Type[] interceptorTypes)
        {
            return delegate(Container c, IDictionary deps)
            {
                object instance = CreateInstance(impl, deps);
                List<IInterceptor> interceptors = new List<IInterceptor>();
                foreach (Type interceptorType in interceptorTypes)
                {
                    interceptors.Add((IInterceptor)Resolve(interceptorType, deps));
                }
                if (interceptors.Count == 0)
                    return instance;
                return ProxyFactory.Create(service, instance, interceptors.ToArray());
            };
        }

        private object TryGetConfiguration(string name)
        {
            object value;
            if (configuration.TryGetValue(name, out value))
            {
                return value;
            }
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

        private object CreateInstance(Type type, IDictionary dictionary)
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
                        if (dictionary.Contains(info.Name))
                        {
                            arg = dictionary[info.Name];
                        }
                        else
                        {
                        	if (IoC.Container.TryGetConfiguration(info.Name) != null)
                        	{
                        		arg = IoC.Container.TryGetConfiguration(info.Name);
                        		arg = Convert.ChangeType(arg, info.ParameterType);
                        	}
                        	else
                        	{
                        		arg = IoC.Resolve(info.ParameterType, dictionary);
                        	}
                        }
                        args.Add(arg);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(
                            "Failed when trying to resolve dependency '" + info.Name + "' for: " + type, e);
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

        private void PerformEnvironmentValidation(Type type, object resolve)
        {
            ICanValidateMyEnvironment validator = resolve as ICanValidateMyEnvironment;
            if (validator == null)
                return;
            if (performedValidation.ContainsKey(type) == false)
            {
                lock (performedValidation)
                {
                    if (performedValidation.ContainsKey(type))
                        return;
                    validator.ValidateEnvironment();
                    performedValidation[type] = true;
                }
            }
        }
    }
}