using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ConsoleTest
{
	internal static class DataReaderHelper
	{
		internal static T? GetFieldValue<T>(this SqlDataReader reader, string name)
		{
			T? ret = default;
			if (!reader[name].Equals(DBNull.Value))
			{
				ret = (T)reader[name];
			}

			return ret;
		}
	}
}
