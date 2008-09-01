using System.Net;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;
using SvnBridge.Cache;

namespace SvnBridge.Infrastructure
{
    public class SourceControlServicesHub : ISourceControlServicesHub
    {
        private readonly ICredentials credentials;
        private readonly ITFSSourceControlService sourceControlService;
        private readonly IProjectInformationRepository projectInformationRepository;
        private readonly IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly ILogger logger;
        private readonly ICache cache;
        private readonly FileCache fileCache;
		private readonly IMetaDataRepositoryFactory metaDataRepositoryFactory;
        private readonly FileRepository fileRepository;

    	public SourceControlServicesHub(ICredentials credentials, ITFSSourceControlService sourceControlService, IProjectInformationRepository projectInformationRepository, IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet, ILogger logger, ICache cache, FileCache fileCache, IMetaDataRepositoryFactory metaDataRepositoryFactory, FileRepository fileRepository)
        {
            this.credentials = credentials;
    		this.metaDataRepositoryFactory = metaDataRepositoryFactory;
            this.sourceControlService = sourceControlService;
            this.projectInformationRepository = projectInformationRepository;
            this.associateWorkItemWithChangeSet = associateWorkItemWithChangeSet;
            this.logger = logger;
            this.cache = cache;
            this.fileCache = fileCache;
            this.fileRepository = fileRepository;
        }

        public ICredentials Credentials
        {
            get { return credentials; }
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

        public FileCache FileCache
        {
            get { return fileCache; }
        }

    	public IMetaDataRepositoryFactory MetaDataRepositoryFactory
    	{
			get { return metaDataRepositoryFactory; }
    	}

        public FileRepository FileRepository
        {
            get { return fileRepository; }
        }
    }
}