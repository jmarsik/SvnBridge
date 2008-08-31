using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge
{
    public class BootStrapper
    {
        private readonly Assembly[] assemblies;
        private readonly string[] componentInterfacesNamespace;

        private readonly Type[] representiveComponents = new Type[]
            {
                // representive interfaces
                typeof (ICache),
                typeof (ISourceControlProvider),
                typeof (IRepositoryWebSvcFactory),
                typeof (IRegistrationWebSvcFactory),
                typeof (IRegistrationService),
                typeof (IFileSystem),
                typeof (IWebTransferService),
                typeof (ITFSSourceControlService),
                typeof (IAssociateWorkItemWithChangeSet),
                typeof (IListener),
                typeof (IActionTracking)
            };

        public BootStrapper()  
		{
            IoC.Container.Configuration["fileCachePath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileCache");
            IoC.Container.Configuration["persistentCachePath"] =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MetaDataCache");

        	TfsUtil.OnSetupWebRequest = WebRequestSetup.OnWebRequest;

            List<Assembly> asms = new List<Assembly>();
            List<string> names = new List<string>();
            foreach (Type type in representiveComponents)
            {
                if (asms.Contains(type.Assembly) == false)
                {
                    asms.Add(type.Assembly);
                }

                if (type.IsInterface) // we perform auto registration for interface only
                {
                    names.Add(type.Namespace);
                }
            }

            componentInterfacesNamespace = names.ToArray();
            assemblies = asms.ToArray();
        }

        public void Start()
        {
            RegisterTypesFromKnownAssemblies();
        	RegisterTypeFromAddinAssemblies();
            RegisterType(typeof(FileRepository), typeof(FileRepository));
        }

    	private void RegisterTypeFromAddinAssemblies()
    	{
    		string[] assemblyNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
    		foreach (string assemblyName in assemblyNames)
    		{
				// Here we assume that the assembly name and the file name are identical.
    			Assembly assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyName));
				// we ignore the well known assemblies
				if(Array.IndexOf(assemblies, assembly)!=-1)
					continue;

				foreach (Type type in assembly.GetTypes())
    			{
					if (typeof(IAssemblyAddin).IsAssignableFrom(type))
    				{
    					IAssemblyAddin addin = (IAssemblyAddin) Activator.CreateInstance(type);
    					addin.Initialize(IoC.Container);
    				}
    			}
    		}
    	}

    	private void RegisterTypesFromKnownAssemblies()
    	{
    		foreach(Type type in GetAllTypesFromAssemblies())
    		{
    			if (ValidTypeForRegistration(type) == false)
    				continue;

    			foreach (Type interfaceType in type.GetInterfaces())
    			{
                    RegisterType(interfaceType, type);
    			}
    		}
    	}

        private void RegisterType(Type interfaceType, Type type)
        {
            if (!IoC.Container.IsRegistered(interfaceType))
            {
                object[] attributes = type.GetCustomAttributes(typeof(InterceptorAttribute), true);
                if (attributes.Length == 0)
                {
                    IoC.Container.Register(interfaceType, type);
                }
                else
                {
                    List<Type> interceptors = new List<Type>();
                    Array.ForEach(attributes, delegate(object attr)
                    {
                        interceptors.Add(((InterceptorAttribute)attr).Interceptor);
                    });
                    IoC.Container.Register(interfaceType, type, interceptors.ToArray());
                }
            }
        }

    	private IEnumerable<Type> GetAllTypesFromAssemblies()
    	{
    		foreach (Assembly assembly in assemblies)
    		{
    			foreach (Type type in assembly.GetTypes())
    			{
    				yield return type;
    			}
    		}
    	}

    	private bool IsValidInterface(Type interfaceType)
        {
            return Array.IndexOf(componentInterfacesNamespace, interfaceType.Namespace) != -1 &&
                   Array.IndexOf(assemblies, interfaceType.Assembly) != -1;
        }


        private bool ValidTypeForRegistration(Type type)
        {
            if (type.Namespace == "SvnBridge.NullImpl")
                return false;

            if (type.IsInterface || type.IsAbstract)
            {
                return false;
            }
            if (Array.IndexOf(representiveComponents, type) != -1)
            {
                return true;
            }
            Type[] interfaces = type.GetInterfaces();
            foreach (Type interfaceType in interfaces)
            {
                if (IsValidInterface(interfaceType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
