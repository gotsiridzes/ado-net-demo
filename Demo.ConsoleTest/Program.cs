using System.Data.SqlClient;
using System.Text;

namespace demo.ConsoleTest
{
	class Program
	{
		public static void Main(string[] args)
		{
			string connectionInformation = string.Empty;
			try
			{
				using (var connection = new SqlConnection("server=localhost;database=B7ConsumerLoans;trusted_connection=true;"))
				{
					connection.Open();
					connectionInformation = GetConnectionInformation(connection);
					int rowCount = Select(connection);

					Console.WriteLine($"Rows Affected: {rowCount}");
				}
			}
			catch (Exception ex)
			{
				connectionInformation = ex.ToString();
			}

			Console.WriteLine(connectionInformation);

			Console.ReadKey();
		}

		private static int Select(SqlConnection connection)
		{
			int rowCount = 0;
			var sql = "SELECT COUNT(*) from consumerloans.Loans";
			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				rowCount = (int)command.ExecuteScalar();
			}

			return rowCount;
		}

		private static string GetConnectionInformation(SqlConnection connection)
		{
			var sb = new StringBuilder(1024);
			sb.AppendLine($"Connection String: {connection.ConnectionString}");
			sb.AppendLine($"State: {connection.State.ToString()}");
			sb.AppendLine($"Connection Timeout: {connection.ConnectionTimeout.ToString()}");
			sb.AppendLine($"Database: {connection.Database}");
			sb.AppendLine($"Data Source: {connection.DataSource}");
			sb.AppendLine($"Server Version: {connection.ServerVersion}");
			sb.AppendLine($"Workstation Id: {connection.WorkstationId}");

			return sb.ToString();
		}
	}
}