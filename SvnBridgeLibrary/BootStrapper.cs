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
using SvnBridge.Cache;

namespace SvnBridge
{
    public class BootStrapper
    {
        public BootStrapper()  
		{
            IoC.Container.Configuration["fileCachePath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileCache");
            IoC.Container.Configuration["persistentCachePath"] =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MetaDataCache");

        	TfsUtil.OnSetupWebRequest = WebRequestSetup.OnWebRequest;
        }

        public void Start()
        {
            RegisterType(typeof(FileRepository), typeof(FileRepository));
            RegisterType(typeof(ISourceControlServicesHub), typeof(SourceControlServicesHub));
            RegisterType(typeof(IFileCache), typeof(FileCache));
            RegisterType(typeof(IPersistentCache), typeof(MemoryBasedPersistentCache));
            RegisterType(typeof(ILogger), typeof(DefaultLogger));
            RegisterType(typeof(IIgnoredFilesSpecification), typeof(OldSvnBridgeFilesSpecification));
            RegisterType(typeof(IActionTracking), typeof(ActionTrackingViaPerfCounter));
            RegisterType(typeof(IMetaDataRepositoryFactory), typeof(MetaDataRepositoryFactory));
            RegisterType(typeof(IListener), typeof(Listener));
            RegisterType(typeof(IAssociateWorkItemWithChangeSet), typeof(AssociateWorkItemWithChangeSet));
            RegisterType(typeof(ICache), typeof(WebCache));
            RegisterType(typeof(ISourceControlProvider), typeof(TFSSourceControlProvider));
            RegisterType(typeof(ITFSSourceControlService), typeof(TFSSourceControlService));
            RegisterType(typeof(IProjectInformationRepository), typeof(ProjectInformationRepository));
            RegisterType(typeof(ITfsUrlValidator), typeof(TfsUrlValidator));
            RegisterType(typeof(IRegistrationService), typeof(RegistrationService));
            RegisterType(typeof(IWebTransferService), typeof(WebTransferService));
            RegisterType(typeof(IRegistrationWebSvcFactory), typeof(RegistrationWebSvcFactory));
            RegisterType(typeof(IRepositoryWebSvcFactory), typeof(RepositoryWebSvcFactory));
            RegisterType(typeof(IFileSystem), typeof(FileSystem));
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
    }
}
