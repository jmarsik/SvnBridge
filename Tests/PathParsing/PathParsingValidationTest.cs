using System;
using System.Reflection;
using Xunit;

namespace SvnBridge.PathParsing
{
	public class PathParsingValidationTest
	{
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

		private void ValidateParserWillNotAcceptInvalidTfsUrl<T>()
		{
			Assert.Throws<InvalidOperationException>(delegate
			{
				try
				{
					Activator.CreateInstance(typeof (T), "blah");
				}
				catch (TargetInvocationException e)
				{
					throw e.InnerException;
				}
			});
		}
	}
}