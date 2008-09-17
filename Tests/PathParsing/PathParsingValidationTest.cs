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
        public void PathParserProjectInDomain_DoesNotAcceptInvalidUrl()
		{
            Exception result = Record.Exception(delegate {
                new PathParserProjectInDomain("blah", stubs.CreateProjectInformationRepositoryStub());
            });

            Assert.NotNull(result);
		}

		[Fact]
		public void StaticServerPathParser_DoesNotAcceptInvalidUrl()
		{
            Exception result = Record.Exception(delegate {
                new PathParserSingleServerWithProjectInPath("blah");
            });

            Assert.NotNull(result);
		}

        [Fact]
        public void PathParserSingleServerWithProjectInPath_AcceptValidUrl()
        {
            Exception result = Record.Exception(delegate
            {
                new PathParserSingleServerWithProjectInPath("https://codeplex.com");
            });

            Assert.Null(result);
        }

        [Fact]
        public void PathParserProjectInDomain_AcceptValidUrl()
        {
            Exception result = Record.Exception(delegate
            {
                new PathParserProjectInDomain("https://codeplex.com", stubs.CreateProjectInformationRepositoryStub());
            });

            Assert.Null(result);
        }

        [Fact]
        public void PathParserProjectInDomain_AcceptValidUrl_Muliple()
        {
            Exception result = Record.Exception(delegate
            {
                new PathParserProjectInDomain("https://codeplex.com,https://www.codeplex.com", stubs.CreateProjectInformationRepositoryStub());
            });

            Assert.Null(result);
        }
	}
}