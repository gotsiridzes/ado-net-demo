using System;
using System.Data.SqlClient;
using System.Security.AccessControl;

namespace Demo.ConsoleTest
{
	public class SqlExceptionManager
	{
		private SqlExceptionManager()
		{
				
		}

		private static SqlExceptionManager _instance;

		public static SqlExceptionManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new SqlExceptionManager();
				}

				return _instance;
			}
			set { _instance = value; }
		}

		public Exception LastException { get; set; }

		public virtual void Publish(Exception ex, SqlCommand cmd, string exceptionMsg)
		{
			LastException = ex;

			if (cmd != null)
			{
				LastException = CreateDbException(ex, cmd, null);

				// TODO: Implement an exception publisher here
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}

		public virtual SqlDataException CreateDbException(Exception ex, SqlCommand cmd, string exceptionMsg)
		{
			SqlDataException exc;
			exceptionMsg = string.IsNullOrEmpty(exceptionMsg) ? string.Empty : exceptionMsg + " - ";

			exc = new SqlDataException(exceptionMsg + ex.Message, ex)
			{
				ConnectionString = cmd.Connection.ConnectionString,
				Database = cmd.Connection.Database,
				Sql = cmd.CommandText,
				CommandParameters = cmd.Parameters,
				WorkstationId = Environment.MachineName
			};

			return exc;
		}
	}
}
