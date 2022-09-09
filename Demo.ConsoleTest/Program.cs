using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Demo.ConsoleTest;
using Demo.ConsoleTest.Models;

namespace demo.ConsoleTest
{
	class Program
	{
		public static void Main(string[] args)
		{
			HandleMultipleResultSetsFromAdventureWorks();
			Console.ReadKey();
		}

		private static void HandleMultipleResultSetsFromAdventureWorks()
		{
			var ds = new DataSet();
			var sql = "SELECT TOP 1000 * FROM Sales.Customer c;\r\nSELECT  * FROM Sales.Store s";
			DataTable customersDt = null;
			DataTable storesDt = null;

			using (var connection =
			       new SqlConnection("server=localhost;database=AdventureWorks2017;trusted_connection=true"))
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = sql;
					using (var adapter = new SqlDataAdapter(command))
					{
						try
						{
							adapter.Fill(ds);
							if (ds.Tables.Count > 0)
							{
								customersDt = ds.Tables[0]; // result of first select;
								storesDt = ds.Tables[1]; // result of sales.store
							}
						}
						catch (Exception e)
						{
							SqlExceptionManager.Instance.Publish(e, command, "test");
							Console.WriteLine(SqlExceptionManager.Instance.LastException);
						}
					}
				}
			}
		}

		private static List<Product> GetProductsFromAdventureWorks()
		{
			var products = new List<Product>();
			int rowsAffected = 0;
			DataTable dt = null;

			var sql = "SELECT * from Production.Product";
			using (var connection = new SqlConnection("server=localhost;database=AdventureWorks2017;trusted_connection=true"))
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = sql;
					using (var adapter = new SqlDataAdapter(command))
					{
						try
						{
							dt = new DataTable();
							adapter.Fill(dt);

							products = ProductsAsList(dt, products);
						}
						catch (Exception e)
						{
							SqlExceptionManager.Instance.Publish(e, command, "test");
							Console.WriteLine(SqlExceptionManager.Instance.LastException);
						}
					}
				}
			}

			return products;
		}

		private static List<Product> ProductsAsList(DataTable dt, List<Product> products)
		{
			if (dt != null && dt.Rows.Count > 0)
			{
				//products = (from row in dt.AsEnumerable()
				//	select new Product
				//	{
				//		Id = row.Field<int>("ProductId"),
				//		Name = row.Field<string>("Name"),
				//		ProductNumber = row.Field<string>("ProductNumber")
				//	}).ToList();

				products = (dt.AsEnumerable()
					.Select(row => new Product
					{
						Id = row.Field<int>("ProductId"),
						Name = row.Field<string>("Name")!,
						ProductNumber = row.Field<string>("ProductNumber")!,
						Color = row.Field<string?>("Color")
					})).ToList();
			}

			return products;
		}

		private static void UseDataAdapter()
		{
			DataTable dataTable = null;
			using (var connection =new SqlConnection("server=localhost;database=AdventureWorksLT2017;trusted_connection=true"))
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT * FROM SalesLT.Customer";
					using (var adapter = new SqlDataAdapter(command))
					{
						try
						{
							dataTable = new DataTable();
							adapter.Fill(dataTable);

							ProcessRowsAndColumns(dataTable);
						}
						catch (Exception e)
						{
							SqlExceptionManager.Instance.Publish(e, command, "test");
							Console.WriteLine(SqlExceptionManager.Instance.LastException);
							
							throw;
						}
					}
				}
			}
		}

		private static void ProcessRowsAndColumns(DataTable dataTable)
		{
			int index = 1;
			StringBuilder sb = new StringBuilder();

			// reading each row
			foreach (DataRow row in dataTable.Rows)
			{
				sb.AppendLine($"** Row: {index} **");
				//reading each column
				foreach (DataColumn column in dataTable.Columns)
					sb.AppendLine($"{column.ColumnName}: {row[column.ColumnName]}");

				sb.AppendLine();
				index++;
			}

			Console.WriteLine(sb);
		}

		private static void Test()
		{
			SqlConnection connection = null;
			SqlCommand command = null;
			string connectionInformation = string.Empty;
			try
			{
				using (connection = new SqlConnection("server=localhost;database=AdventureWorksLT2017;trusted_connection=true;"))
				{
					connection.Open();
					connectionInformation = GetConnectionInformation(connection);
					decimal amount = Select(connection, command, 1);

					Console.WriteLine($"loan amount: {amount}");
				}
			}
			catch (Exception ex)
			{
				connectionInformation = ex.ToString();
			}

			GetLoanData(connection, command, 1);

			Console.WriteLine(connectionInformation);
		}

		private static decimal Select(SqlConnection connection, SqlCommand command, int id)
		{
			decimal amount = 0m;
			var sql = "SELECT Amount from consumerloans.Loans WHERE Id = @Id";
			using (command = connection.CreateCommand())
			{
				command.CommandText = sql;
				command.Parameters.Add(new SqlParameter("@Id", id));
				amount = (decimal)command.ExecuteScalar();
			}

			return amount;
		}

		private static void GetLoanData(SqlConnection connection, SqlCommand command, int id)
		{
			try
			{
				using (connection = new SqlConnection("server=locaslhost;database=AdventureWorksLT2017;trusted_connection=true;"))
				{
					using (command = connection.CreateCommand())
					{
						command.CommandTimeout = 0;
						// handling tow result sets are more effective than asking db for data two times.
						command.CommandText =
							"select * from loan.LOANS where LOAN_ID = @idd; select * from loan.LOANS where LOAN_ID = @id + 1";
						command.Parameters.Add(new SqlParameter("@id", id));

						connection.Open();
						using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
						{
							while (reader.Read())
							{
								Console.WriteLine(reader["LOAN_ID"]);
								Console.WriteLine(reader.GetDecimal(reader.GetOrdinal("AMOUNT")));
								Console.WriteLine(reader.GetFieldValue<decimal>(reader.GetOrdinal("USED_AMOUNT")));
								Console.WriteLine(reader.GetFieldValue<decimal?>("FEE1"));
							}

							reader.NextResult();
							// next result set; i.e. multiple select statements;
							while (reader.Read())
							{
								Console.WriteLine(reader.GetFieldValue<int>(reader.GetOrdinal("LOAN_ID")));
								Console.WriteLine(reader.GetDecimal(reader.GetOrdinal("AMOUNT")));
								Console.WriteLine(reader.GetFieldValue<decimal?>("USED_AMOUNT"));
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				SqlExceptionManager.Instance.Publish(ex, command, "hello world");
				Console.WriteLine(SqlExceptionManager.Instance.LastException.ToString());
				//StringBuilder sb = new StringBuilder();
				//for (int i = 0; i < ex.Errors.Count; i++)
				//{
				//	sb.AppendLine($"Index: {i.ToString()}");
				//	sb.AppendLine($"Type: {ex.Errors[i].GetType().FullName}");
				//	sb.AppendLine($"Message: {ex.Errors[i].Message}");
				//	sb.AppendLine($"Source: {ex.Errors[i].Source}");
				//	sb.AppendLine($"Number: {ex.Errors[i].Number}");
				//	sb.AppendLine($"State: {ex.Errors[i].State}");
				//	sb.AppendLine($"Class: {ex.Errors[i].Class}");
				//	sb.AppendLine($"Server: {ex.Errors[i].Server}");
				//	sb.AppendLine($"Procedure: {ex.Errors[i].Procedure}");
				//	sb.AppendLine($"Line Number: {ex.Errors[i].LineNumber}");
				//}

				//Console.WriteLine(sb.ToString()+ Environment.NewLine + ex);
			}
			finally
			{
				if (connection != null)
				{
					connection.Close();
					connection.Dispose();
				}

				if (command != null)
				{
					command.Dispose();
				}
			}
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