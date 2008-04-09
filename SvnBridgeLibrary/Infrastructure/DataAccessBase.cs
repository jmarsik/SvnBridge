using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;

namespace SvnBridge.Infrastructure
{
	public class DataAccessBase
	{
		public delegate void Action();

		private IDbConnection connection;
		private IDbTransaction transaction;
		protected string connectionString;

		public DataAccessBase(string connectionString)
		{
			this.connectionString = connectionString;
		}


		[DebuggerNonUserCode]
		[DebuggerStepThrough]
		protected void Transaction(IsolationLevel isolationLevel, Action action)
		{
			bool responsibleForClosingConnection = connection == null;
			connection = connection ?? new SqlCeConnection(connectionString);
			try
			{
				if (responsibleForClosingConnection)
					connection.Open();

				bool responsibleForCommitedTransaction = transaction == null;
				transaction = transaction ?? connection.BeginTransaction(isolationLevel);
				try
				{
					action();
					if (responsibleForCommitedTransaction)
						transaction.Commit();
				}
				catch
				{
					if (responsibleForCommitedTransaction)
						transaction.Rollback();
					throw;
				}
				finally
				{
					if(responsibleForCommitedTransaction)
					{
						transaction.Dispose();
						transaction = null;
					}
				}
			}
			finally
			{
				if(responsibleForClosingConnection)
				{
					connection.Dispose();
					connection = null;
				}
			}
		}

		protected void ExecuteCommands(string[] commands, IDbCommand command)
		{
			foreach (string sql in commands)
			{
				command.CommandText = sql.Trim();
				if (string.IsNullOrEmpty(command.CommandText))
					continue;
				command.ExecuteNonQuery();
			}
		}


		[DebuggerNonUserCode]
		[DebuggerStepThrough]
		protected void TransactionalCommand(IsolationLevel isolationLevel, Action<IDbCommand> action)
		{
			Transaction(isolationLevel, delegate
			{
				Command(action);
			});
		}

		[DebuggerNonUserCode]
		[DebuggerStepThrough]
		protected void Command(Action<IDbCommand> action)
		{
			using (IDbCommand command = connection.CreateCommand())
			{
				command.Transaction = transaction;
				action(command);
			}
		}

		[DebuggerNonUserCode]
		[DebuggerStepThrough]
		protected static void Parameter(IDbCommand command, string name, object value)
		{
			IDbDataParameter parameter = command.CreateParameter();
			parameter.ParameterName = "@" + name;
			parameter.Value = value ?? DBNull.Value;
			command.Parameters.Add(parameter);
		}

	}
}