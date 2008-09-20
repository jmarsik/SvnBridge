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
        private readonly AssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly DefaultLogger logger;
        private readonly WebCache cache;
		private readonly MetaDataRepositoryFactory metaDataRepositoryFactory;
        private readonly FileRepository fileRepository;

        public SourceControlServicesHub(ICredentials credentials, TFSSourceControlService sourceControlService, AssociateWorkItemWithChangeSet associateWorkItemWithChangeSet, DefaultLogger logger, WebCache cache, MetaDataRepositoryFactory metaDataRepositoryFactory, FileRepository fileRepository)
        {
            this.credentials = credentials;
    		this.metaDataRepositoryFactory = metaDataRepositoryFactory;
            this.sourceControlService = sourceControlService;
            this.associateWorkItemWithChangeSet = associateWorkItemWithChangeSet;
            this.logger = logger;
            this.cache = cache;
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