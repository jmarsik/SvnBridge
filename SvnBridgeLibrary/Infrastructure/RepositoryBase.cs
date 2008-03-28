using System;
using System.Data;
using System.Data.SqlServerCe;

namespace SvnBridge.Infrastructure
{
	public class RepositoryBase
	{
		private IDbConnection connection;
		private IDbTransaction transaction;
		protected string connectionString;

		public RepositoryBase(string connectionString)
		{
			this.connectionString = connectionString;
		}


		public delegate void Action();

		protected void Transaction(Action action)
		{
			bool responsibleForClosingConnection = connection == null;
			connection = connection ?? new SqlCeConnection(connectionString);
			try
			{
				if (responsibleForClosingConnection)
					connection.Open();

				bool responsibleForCommitedTransaction = transaction == null;
				transaction = transaction ?? connection.BeginTransaction(IsolationLevel.ReadCommitted);
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

		protected void TransactionalCommand(Action<IDbCommand> action)
		{
			Transaction(delegate
			{
				Command(action);
			});
		}

		protected void Command(Action<IDbCommand> action)
		{
			using (IDbCommand command = connection.CreateCommand())
			{
				action(command);
			}
		}

		protected static void Parameter(IDbCommand command, string name, object value)
		{
			IDbDataParameter parameter = command.CreateParameter();
			parameter.ParameterName = "@" + name;
			parameter.Value = value ?? DBNull.Value;
			command.Parameters.Add(parameter);
		}

	}
}