using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MassTransit.Gateway.SqlServer.Database
{
    public class DbSchemaReader
    {
        public static async Task<IEnumerable<DbColumnInfo>> ReadTableSchema(Func<SqlConnection> connectionFactory,
            string tableName)
        {
            var query = $"SELECT * FROM {tableName}";
            using (var connection = connectionFactory())
            using (var command = new SqlCommand(query, connection))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().ConfigureAwait(false);

                DataTable schema;
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    schema = reader.GetSchemaTable();
                }

                return schema.Rows.Cast<DataRow>().Select(row =>
                    new DbColumnInfo
                    {
                        Name = row["ColumnName"].ToString(),
                        DbType = row["DataType"].ToString()
                    });
            }
        }

        public class DbColumnInfo
        {
            public string Name { get; set; }
            public string DbType { get; set; }
        }
    }
}