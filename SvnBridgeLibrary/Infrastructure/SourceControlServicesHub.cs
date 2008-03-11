using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Infrastructure
{
    public class SourceControlServicesHub : ISourceControlServicesHub
    {
        private readonly ICredentials credentials;
        private readonly IWebTransferService webTransferService;
        private readonly ITFSSourceControlService sourceControlService;
        private readonly IProjectInformationRepository projectInformationRepository;
        private readonly IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly ILogger logger;
        private readonly ICache cache;
        private readonly IFileCache fileCache;


        public SourceControlServicesHub(ICredentials credentials, IWebTransferService webTransferService, ITFSSourceControlService sourceControlService, IProjectInformationRepository projectInformationRepository, IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet, ILogger logger, ICache cache, IFileCache fileCache)
        {
            this.credentials = credentials;
            this.webTransferService = webTransferService;
            this.sourceControlService = sourceControlService;
            this.projectInformationRepository = projectInformationRepository;
            this.associateWorkItemWithChangeSet = associateWorkItemWithChangeSet;
            this.logger = logger;
            this.cache = cache;
            this.fileCache = fileCache;
        }

        public ICredentials Credentials
        {
            get { return credentials; }
        }

        public IWebTransferService WebTransferService
        {
            get { return webTransferService; }
        }

        public ITFSSourceControlService SourceControlService
        {
            get { return sourceControlService; }
        }

        public IProjectInformationRepository ProjectInformationRepository
        {
            get { return projectInformationRepository; }
        }

        public IAssociateWorkItemWithChangeSet AssociateWorkItemWithChangeSet
        {
            get { return associateWorkItemWithChangeSet; }
        }

        public ILogger Logger
        {
            get { return logger; }
        }

        public ICache Cache
        {
            get { return cache; }
        }

        public IFileCache FileCache
        {
            get { return fileCache; }
        }
    }
}