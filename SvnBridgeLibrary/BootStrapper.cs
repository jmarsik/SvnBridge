using System;
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
                typeof (IAssociateWorkItemWithChangeSet)
            };

        public BootStrapper()
        {
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
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (ValidTypeForRegistration(type) == false)
                    {
                        continue;
                    }


                    foreach (Type interfaceType in type.GetInterfaces())
                    {
                        if (IoC.Container.IsRegistered(interfaceType))
                        {
                            continue;
                        }

                        IoC.Container.Register(interfaceType, type);
                    }
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