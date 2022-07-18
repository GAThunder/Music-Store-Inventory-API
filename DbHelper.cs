using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InventoryAPI
{
    public static class DbHelper
    {
        public static readonly string _connectionString = "Data Source=localhost;Initial Catalog=Inventory;Integrated Security=SSPI;Connection Timeout=20";
		private static readonly object _lockObject = new object();

		/// <summary>
		/// Returns a generic list based on a select statement.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="IsStoredProc"></param>
		/// <returns></returns>
		public static List<T> Retrieve<T>(string sql, Dictionary<string, object> parameters = null, bool isStoredProc = true) where T : class
		{
			SqlCommand command = CreateCommand(sql, parameters, isStoredProc);

			return Retrieve<T>(command);
		}

		/// <summary>
		/// Runs a non query based on the sql and parameters passed.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="isStoredProc"></param>
		public static void ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null, bool isStoredProc = true)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				using (SqlCommand command = connection.CreateCommand())
				{
					if (isStoredProc)
					{
						command.CommandType = CommandType.StoredProcedure;
					}
					command.CommandText = sql;
					command.CommandTimeout = connection.ConnectionTimeout;

					AddParameters(command, parameters);

					command.ExecuteNonQuery();
				}
			}
		}

		private static SqlCommand CreateCommand(string sql, Dictionary<string, object> parameters = null, bool isStoredProc = true)
		{
			SqlCommand command = new SqlCommand(sql);

			if (isStoredProc)
			{
				command.CommandType = CommandType.StoredProcedure;
			}

			AddParameters(command, parameters);

			return command;
		}

		private static void AddParameters(SqlCommand command, Dictionary<string, object> parameters)
        {
			if (parameters != null)
			{
				foreach (string key in parameters.Keys)
				{
					object value = parameters[key];
					DbParameter parameter = command.CreateParameter();

					parameter.ParameterName = $"@{key}";
					parameter.Value = value ?? DBNull.Value;
					command.Parameters.Add(parameter);
				}
			}
		}

		private static List<T> Retrieve<T>(SqlCommand command) where T : class
		{
			List<T> objects;

			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				command.Connection = connection;
				command.CommandTimeout = connection.ConnectionTimeout;

				objects = ExecuteRetrieve<T>(command);
			}

			return objects ?? new List<T>();
		}

		private static List<T> ExecuteRetrieve<T>(SqlCommand command)
		{
			List<T> objects = new List<T>();
			DataSet ds = new DataSet();

			using (SqlDataAdapter adapter = new SqlDataAdapter(command))
			{
				adapter.Fill(ds);
			}


			if (ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
			{
				DataTable table = ds.Tables[0];
				DataColumnCollection columns = table.Columns;

				Parallel.ForEach(table.Rows.Cast<DataRow>(),
					row => { PopulateObjectByDataRow(columns, objects, row); });
			}

			return objects;
		}

		private static void PopulateObjectByDataRow<T>(DataColumnCollection columns, List<T> objects, DataRow row)
		{
			T Object = GetResult<T>(columns, row);

			lock (_lockObject)
			{
				objects.Add(Object);
			}
		}

		private static T GetResult<T>(DataColumnCollection columns, DataRow row)
        {
			T result = (T)Activator.CreateInstance(typeof(T));
			PropertyInfo[] propertries = typeof(T).GetProperties();
			for (int i = 0; i < row.ItemArray.Length; ++i)
			{
				DataColumn column = columns[i];

				if (propertries.Any(p => p.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)))
				{
					PropertyInfo propertyInfo = propertries.FirstOrDefault(p => p.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase));
					if (propertyInfo != null)
					{
						object value = row[column];
						Type type = value.GetType();

						if (type == typeof(DBNull))
						{
							propertyInfo.SetValue(result, null);
						}
						else
						{
							propertyInfo.SetValue(result, value);
						}
					}
				}
			}

			return result;
		}
	}
}
