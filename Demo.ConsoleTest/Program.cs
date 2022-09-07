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
					decimal amount = Select(connection, 1);

					Console.WriteLine($"loan amount: {amount}");
				}
			}
			catch (Exception ex)
			{
				connectionInformation = ex.ToString();
			}

			Console.WriteLine(connectionInformation);

			Console.ReadKey();
		}

		private static decimal Select(SqlConnection connection, int id)
		{
			decimal amount = 0m;
			var sql = "SELECT Amount from consumerloans.Loans WHERE Id = @Id";
			using (var command = connection.CreateCommand())
			{
				command.CommandText = sql;
				command.Parameters.Add(new SqlParameter("@Id", id));
				amount = (decimal)command.ExecuteScalar();
			}

			return amount;
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