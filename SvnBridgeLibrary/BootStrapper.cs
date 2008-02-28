using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge
{
    public class BootStrapper
    {
        private readonly string[] componentInterfacesNamespace;
        private readonly Assembly[] assemblies;

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

                // concrete types
                typeof (TFSSourceControlService)
            };

        public BootStrapper()
        {
            List<Assembly> asms = new List<Assembly>();
            List<string> names = new List<string>();
            foreach (Type type in representiveComponents)
            {
                if (asms.Contains(type.Assembly) == false)
                    asms.Add(type.Assembly);

                if(type.IsInterface)// we perform auto registration for interface only
                    names.Add(type.Namespace);
            }

            componentInterfacesNamespace = names.ToArray();
            assemblies = asms.ToArray();
        }

        public void Start()
        {
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (ValidTypeForRegistration(type) == false)
                        continue;

                    Type interfaceType = GetInterfaceType(type);

                    if (IoC.Container.IsRegistered(interfaceType))
                        continue;

                    IoC.Container.Register(interfaceType, type);
                }
            }
        }

        private Type GetInterfaceType(Type type)
        {
            if (Array.IndexOf(representiveComponents, type) != -1)
                return type;
            Type selected = null;
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (IsValidInterface(interfaceType))
                {
                    if (selected != null)
                    {
                        throw new InvalidOperationException("The component " + type +
                                                            " implements more than a single interface. SoC violation.");
                    }
                    selected = interfaceType;
                }
            }
            return selected;
        }

        private bool IsValidInterface(Type interfaceType)
        {
            return Array.IndexOf(componentInterfacesNamespace, interfaceType.Namespace) != -1;
        }

      
        private bool ValidTypeForRegistration(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
                return false;
            if (Array.IndexOf(representiveComponents, type) != -1)
                return true;
            Type[] interfaces = type.GetInterfaces();
            foreach (Type interfaceType in interfaces)
            {
                if (IsValidInterface(interfaceType))
                    return true;
            }
            return false;
        }
    }
}