using System;
using System.Reflection;
using Rhino.Mocks;
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
			ValidateParserWillNotAcceptInvalidTfsUrl<StaticServerWithProjectNameInHostNamePathParser>();
		}

		[Fact]
		public void StaticServerPathParser_DoesNotAcceptInvalidUrl()
		{
			ValidateParserWillNotAcceptInvalidTfsUrl<StaticServerPathParser>();
		}

        [Fact]
        public void StaticServerPathParser_AcceptValidUrl()
        {
            ValidateParserWillAcceptValidTfsUrl<StaticServerPathParser>("https://codeplex.com");
        }

        [Fact]
        public void StaticServerWithProjectNameInHostNamePathParser_AcceptValidUrl()
        {
            ValidateParserWillAcceptValidTfsUrl<StaticServerWithProjectNameInHostNamePathParser>("https://codeplex.com");
        }

        [Fact]
        public void StaticServerPathParser_AcceptValidUrl_Muliple()
        {
            ValidateParserWillAcceptValidTfsUrl<StaticServerPathParser>("https://codeplex.com,https://www.codeplex.com");
        }

        [Fact]
        public void StaticServerWithProjectNameInHostNamePathParser_AcceptValidUrl_Muliple()
        {
            ValidateParserWillAcceptValidTfsUrl<StaticServerWithProjectNameInHostNamePathParser>("https://codeplex.com,https://www.codeplex.com");
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