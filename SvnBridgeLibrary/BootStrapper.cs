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
        	TfsUtil.OnSetupWebRequest = WebRequestSetup.OnWebRequest;
        }

        public void Start()
        {
            Container.Register(typeof(IRegistrationService), typeof(RegistrationService));
            Container.Register(typeof(IWebTransferService), typeof(WebTransferService));
            Container.Register(typeof(IRegistrationWebSvcFactory), typeof(RegistrationWebSvcFactory));
            Container.Register(typeof(IRepositoryWebSvcFactory), typeof(RepositoryWebSvcFactory));
            Container.Register(typeof(IFileSystem), typeof(FileSystem));
        }
    }
}