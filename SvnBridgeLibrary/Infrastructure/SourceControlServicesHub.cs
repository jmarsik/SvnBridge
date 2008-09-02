using System.Net;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;
using SvnBridge.Cache;

namespace SvnBridge.Infrastructure
{
    public class SourceControlServicesHub
    {
        private readonly ICredentials credentials;
        private readonly TFSSourceControlService sourceControlService;
        private readonly ProjectInformationRepository projectInformationRepository;
        private readonly AssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly DefaultLogger logger;
        private readonly WebCache cache;
        private readonly FileCache fileCache;
		private readonly MetaDataRepositoryFactory metaDataRepositoryFactory;
        private readonly FileRepository fileRepository;

        public SourceControlServicesHub(ICredentials credentials, TFSSourceControlService sourceControlService, ProjectInformationRepository projectInformationRepository, AssociateWorkItemWithChangeSet associateWorkItemWithChangeSet, DefaultLogger logger, WebCache cache, FileCache fileCache, MetaDataRepositoryFactory metaDataRepositoryFactory, FileRepository fileRepository)
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

        public TFSSourceControlService SourceControlService
        {
            get { return sourceControlService; }
        }

        public ProjectInformationRepository ProjectInformationRepository
        {
            get { return projectInformationRepository; }
        }

        public AssociateWorkItemWithChangeSet AssociateWorkItemWithChangeSet
        {
            get { return associateWorkItemWithChangeSet; }
        }

        public DefaultLogger Logger
        {
            get { return logger; }
        }

        public WebCache Cache
        {
            get { return cache; }
        }

        public FileCache FileCache
        {
            get { return fileCache; }
        }

    	public MetaDataRepositoryFactory MetaDataRepositoryFactory
    	{
			get { return metaDataRepositoryFactory; }
    	}

        public FileRepository FileRepository
        {
            get { return fileRepository; }
        }
    }
}