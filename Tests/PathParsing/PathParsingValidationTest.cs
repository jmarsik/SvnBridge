using System;
using System.Reflection;
using SvnBridge.Interfaces;
using Xunit;
using SvnBridge.SourceControl;
using Tests;

namespace SvnBridge.PathParsing
{
	public class PathParsingValidationTest
	{
        protected MyMocks stubs = new MyMocks();
        
        [Fact]
		public void StaticServerWithProjectNameInHostNamePathParser_DoesNotAcceptInvalidUrl()
		{
			ValidateParserWillNotAcceptInvalidTfsUrl<PathParserProjectInDomain>();
		}

		[Fact]
		public void StaticServerPathParser_DoesNotAcceptInvalidUrl()
		{
			ValidateParserWillNotAcceptInvalidTfsUrl<PathParserProjectInPath>();
		}

        [Fact]
        public void StaticServerPathParser_AcceptValidUrl()
        {
            ValidateParserWillAcceptValidTfsUrl<PathParserProjectInPath>("https://codeplex.com");
        }

        [Fact]
        public void StaticServerWithProjectNameInHostNamePathParser_AcceptValidUrl()
        {
            ValidateParserWillAcceptValidTfsUrl<PathParserProjectInDomain>("https://codeplex.com");
        }

        [Fact]
        public void StaticServerPathParser_AcceptValidUrl_Muliple()
        {
            ValidateParserWillAcceptValidTfsUrl<PathParserProjectInPath>("https://codeplex.com,https://www.codeplex.com");
        }

        [Fact]
        public void StaticServerWithProjectNameInHostNamePathParser_AcceptValidUrl_Muliple()
        {
            ValidateParserWillAcceptValidTfsUrl<PathParserProjectInDomain>("https://codeplex.com,https://www.codeplex.com");
        }


		private void ValidateParserWillNotAcceptInvalidTfsUrl<T>()
		{
			Assert.Throws<InvalidOperationException>(delegate
			{
				try
				{
					Activator.CreateInstance(typeof (T), "blah", stubs.CreateObject<ProjectInformationRepository>(null, null));
				}
				catch (TargetInvocationException e)
				{
					throw e.InnerException;
				}
			});
		}

        private void ValidateParserWillAcceptValidTfsUrl<T>(string url)
        {
            Activator.CreateInstance(typeof(T), url, stubs.CreateObject<ProjectInformationRepository>(null, null));
        }
	}
}