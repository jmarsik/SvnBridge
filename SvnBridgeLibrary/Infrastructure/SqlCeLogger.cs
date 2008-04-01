using System;
using System.Data;
using System.Data.SqlServerCe;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
	public class SqlCeLogger : DataAccessBase, ILogger, ICanValidateMyEnvironment
	{
		public SqlCeLogger(string loggerConnectionString)
			: base(loggerConnectionString)
		{
		}

		#region ILogger Members

		public void Error(string message, Exception exception)
		{
			Log("Error", message, exception.ToString());
		}

		public void Info(string message, Exception exception)
		{
			Log("Info", message, exception.ToString());
		}

		public void Trace(string message, params object[] args)
		{
			if (Logging.TraceEnabled == false)
				return;
			Log("Trace", string.Format(message, args), null);
		}

		#endregion

		private void Log(string level, string message, string exception)
		{
			TransactionalCommand(delegate(IDbCommand command)
			{
				command.CommandText = Queries.InsertLog;
				Parameter(command, "Level", level);
				Parameter(command, "Message", message);
				Parameter(command, "Exception", exception);
				command.ExecuteNonQuery();
			});
		}

		public void EnsureDbExists()
		{
			try
			{
				Transaction(delegate
				{
					//empty transaction block to verify that we can access DB
				});
			}
			catch
			{
				CreateDatabase();
			}
		}

		public void CreateDatabase()
		{
			SqlCeEngine engine = new SqlCeEngine(connectionString);
			engine.CreateDatabase();

			TransactionalCommand(delegate(IDbCommand command)
			{
				ExecuteCommands(Queries.CreateLoggingDatabase.Split(new char[] { ';' }, StringSplitOptions.None), command);
			});
		}

		#region ICanValidateMyEnvironment Members

		public void ValidateEnvironment()
		{
			EnsureDbExists();
		}

		#endregion
	}
}