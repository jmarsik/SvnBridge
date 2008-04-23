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
			if (Skip != null)
				yield return new SkipCommand(method, "");

			foreach (ITestCommand command in GetTestCommandsFromBase(method))
			{
				using(new ConsoleColorer(ConsoleColor.Gray))
				{
					Console.WriteLine("Test (UsingStaticServerPathParser): {0}", method);
				}
				yield return new UsingStaticServerPathParser(command);
				using (new ConsoleColorer(ConsoleColor.Gray))
				{
					Console.WriteLine("Test (UsingRequestBasePathParser): {0}", method);
				}
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
			string testUrl = "http://" + IPAddress.Loopback + ":" + test.Port + 
				"/SvnBridgeTesting" + test.TestPath;
			test.Initialize(testUrl, new StaticServerPathParser(test.ServerUrl));
			try
			{
				return command.Execute(testClass);
			}
			catch (Exception e)
			{
				using(new ConsoleColorer(ConsoleColor.Red))
				{
					Console.WriteLine("Failed: {0}", e.Message);
				}
				throw;
			}
		}

		public string Name
		{
			get { return command.Name; }
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
			try
			{
				return command.Execute(testClass);
			}
			catch (Exception e)
			{
				using (new ConsoleColorer(ConsoleColor.Red))
				{
					Console.WriteLine("Failed: {0}", e.Message);
				}
				throw;
			}
		}

		public string Name
		{
			get { return command.Name; }
		}

		#endregion

	}

	public class ConsoleColorer : IDisposable
	{
		public ConsoleColorer(ConsoleColor newColor)
		{
			this.oldColor = Console.ForegroundColor;
			Console.ForegroundColor = newColor;
		}

		private ConsoleColor oldColor;

		#region IDisposable Members

		public void Dispose()
		{
			Console.ForegroundColor = oldColor;
		}

		#endregion
	}
}