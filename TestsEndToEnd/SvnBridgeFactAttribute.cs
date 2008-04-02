using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using SvnBridge.Cache;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.PathParsing;
using Xunit;
using Xunit.Sdk;

namespace TestsEndToEnd
{
	public class SvnBridgeFactAttribute : FactAttribute
	{
		protected override IEnumerable<ITestCommand> EnumerateTestCommands(MethodInfo method)
		{
			foreach (ITestCommand command in GetTestCommandsFromBase(method))
			{
				yield return new UsingStaticServerPathParser(command);
				yield return new UsingRequestBasePathParser(command);
			}
		}

		private IEnumerable<ITestCommand> GetTestCommandsFromBase(MethodInfo method)
		{
			return base.EnumerateTestCommands(method);
		}
	}

	internal class UsingStaticServerPathParser : ITestCommand
	{
		private readonly ITestCommand command;

		public UsingStaticServerPathParser(ITestCommand command)
		{
			this.command = command;
		}

		#region ITestCommand Members

		public MethodResult Execute(object testClass)
		{
			var test = (EndToEndTestBase) testClass;
			string testUrl = "http://" + IPAddress.Loopback + ":" + test.Port + "/SvnBridgeTesting" + test.TestPath;
			(test).Initialize(testUrl, new StaticServerPathParser(test.ServerUrl));
			return command.Execute(testClass);
		}

		public string Name
		{
			get { return command.Name; }
		}

		public object[] Parameters
		{
			get { return command.Parameters; }
		}

		#endregion
	}

	internal class UsingRequestBasePathParser : ITestCommand
	{
		private readonly ITestCommand command;

		public UsingRequestBasePathParser(ITestCommand command)
		{
			this.command = command;
		}

		#region ITestCommand Members

		public MethodResult Execute(object testClass)
		{
			var test = (EndToEndTestBase) testClass;
			string testUrl = "http://" + IPAddress.Loopback + ":" + test.Port + "/" +
			                 (new Uri(test.ServerUrl).Host + ":" + new Uri(test.ServerUrl).Port)
			                 + "/SvnBridgeTesting" + test.TestPath;

			IPathParser parser = new RequestBasePathParser(new TfsUrlValidator(new WebCache()));

			((EndToEndTestBase) testClass).Initialize(testUrl, parser);
			return command.Execute(testClass);
		}

		public string Name
		{
			get { return command.Name; }
		}

		public object[] Parameters
		{
			get { return command.Parameters; }
		}

		#endregion
	}
}