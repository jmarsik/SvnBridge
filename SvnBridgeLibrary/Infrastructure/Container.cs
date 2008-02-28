using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SvnBridge.Infrastructure
{
    public class Container
    {
        public delegate object Creator(Container c, IDictionary dependencies);

        private readonly Dictionary<string, object> configuration
            = new Dictionary<string, object>();

        private readonly Dictionary<Type, Creator> typeToCreator
            = new Dictionary<Type, Creator>();

        public Dictionary<string, object> Configuration
        {
            get { return configuration; }
        }

        public void Register<T>(Creator creator)
        {
            Register(typeof (Type), creator);
        }

        public void Register(Type service, Type impl)
        {
            Register(service, delegate(Container c, IDictionary deps)
                                  {
                                      return CreateInstance(impl, deps);
                                  });
        }

        public void Register(Type type, Creator creator)
        {
            if (IsRegistered(type))
                throw new InvalidOperationException(type + " was already registered in the container");
            typeToCreator.Add(type, creator);
        }

        public bool IsRegistered(Type type)
        {
            return typeToCreator.ContainsKey(type);
        }

        public T Resolve<T>(IDictionary dependencies)
        {
            return (T) Resolve(typeof (T), dependencies);
        }

        public object Resolve(Type type, IDictionary dependencies)
        {
            Creator creator;
            if (typeToCreator.TryGetValue(type, out creator) == false)
                throw new InvalidOperationException("No component registered for " + type);
            return creator(this, dependencies);
        }

        public T TryGetConfiguration<T>(string name)
        {
            return (T) configuration[name];
        }

        public object TryGetConfiguration(string name)
        {
            object value;
            if (configuration.TryGetValue(name, out value))
                return value;
            return null;
        }

        private static object CreateInstance(Type type, IDictionary dictionary)
        {
            List<object> args = new List<object>();
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length != 0)
            {
                foreach (ParameterInfo info in constructors[0].GetParameters())
                {
                    try
                    {
                        object arg = dictionary[info.Name] ??
                                     IoC.Container.TryGetConfiguration(info.Name) ??
                                     IoC.Resolve(info.ParameterType, dictionary);
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
                return Activator.CreateInstance(type, args.ToArray());
            else
                return Activator.CreateInstance(type);
        }
    }
}