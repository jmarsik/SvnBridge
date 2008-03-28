using System;
using System.Net;
using System.Web.Services.Protocols;
using CodePlex.TfsLibrary;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Exceptions;

namespace SvnBridge.SourceControl
{
	public class RepositoryFactoryHelper
	{
		private readonly IRepositoryWebSvcFactory webSvcFactory;

		public RepositoryFactoryHelper(IRepositoryWebSvcFactory webSvcFactory)
		{
			this.webSvcFactory = webSvcFactory;
		}

		public Repository TryCreateProxy(string tfsUrl,
		                                 ICredentials credentials)
		{
			try
			{
				Repository repository = (Repository)webSvcFactory.Create(tfsUrl, credentials);
				repository.PreAuthenticate = true;
				repository.UnsafeAuthenticatedConnectionSharing = true;
				return repository;
			}
			catch (SoapException soapEx)
			{
				throw new RepositoryUnavailableException(
					"Failed when accessing server at: '" + tfsUrl + "' reason: " + soapEx.Detail.OuterXml, soapEx);
			}
			catch (NetworkAccessDeniedException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new RepositoryUnavailableException("Failed when access server at: " + tfsUrl, e);
			}
		}
	}
}