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
            IoC.Container.Configuration["persistentCachePath"] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MetaDataCache");
        	TfsUtil.OnSetupWebRequest = WebRequestSetup.OnWebRequest;
        }

        public void Start()
        {
            RegisterType(typeof(FileRepository), typeof(FileRepository));
            RegisterType(typeof(SourceControlServicesHub), typeof(SourceControlServicesHub));
            RegisterType(typeof(FileCache), typeof(FileCache));
            RegisterType(typeof(MemoryBasedPersistentCache), typeof(MemoryBasedPersistentCache));
            RegisterType(typeof(DefaultLogger), typeof(DefaultLogger));
            RegisterType(typeof(ActionTrackingViaPerfCounter), typeof(ActionTrackingViaPerfCounter));
            RegisterType(typeof(MetaDataRepositoryFactory), typeof(MetaDataRepositoryFactory));
            RegisterType(typeof(Listener), typeof(Listener));
            RegisterType(typeof(AssociateWorkItemWithChangeSet), typeof(AssociateWorkItemWithChangeSet));
            RegisterType(typeof(WebCache), typeof(WebCache));
            RegisterType(typeof(TFSSourceControlProvider), typeof(TFSSourceControlProvider));
            RegisterType(typeof(TFSSourceControlService), typeof(TFSSourceControlService));
            RegisterType(typeof(ProjectInformationRepository), typeof(ProjectInformationRepository));
            RegisterType(typeof(TfsUrlValidator), typeof(TfsUrlValidator));
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
