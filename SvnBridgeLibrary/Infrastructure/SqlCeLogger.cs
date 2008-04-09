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

		public void TraceMessage(string message)
		{
			if (Logging.TraceEnabled == false)
				return;
			Log("TraceMessage", message, null);
		}

		#endregion

		private void Log(string level, string message, string exception)
		{
			try
			{
				TransactionalCommand(IsolationLevel.ReadCommitted, delegate(IDbCommand command)
				{
					command.CommandText = Queries.InsertLog;
					Parameter(command, "Level", level);
					Parameter(command, "Message", message);
					Parameter(command, "Exception", exception);
					command.ExecuteNonQuery();
				});
			}
			catch (Exception)
			{
				//We don't have anything to do here, can't 
				// fix errors in error handling code
			}
		}

		public void EnsureDbExists()
		{
			try
			{
				Transaction(IsolationLevel.Serializable, delegate
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
			try
			{
				SqlCeEngine engine = new SqlCeEngine(connectionString);
				engine.CreateDatabase();

				TransactionalCommand(IsolationLevel.Serializable, delegate(IDbCommand command)
				{
					ExecuteCommands(Queries.CreateLoggingDatabase.Split(new char[] { ';' }, StringSplitOptions.None), command);
				});
			}
			catch (Exception)
			{
				// if we fail, nothing much we can do
			}
		}

		#region ICanValidateMyEnvironment Members

		public void ValidateEnvironment()
		{
			EnsureDbExists();
		}

		#endregion
	}
}