using System;
using System.Collections.Generic;
using System.Data;

namespace MassTransit.Gateway.Sql.Database
{
    public class DbSchemaReader
    {
        public static IEnumerable<DbColumnInfo> ReadTableSchema(Func<IDbConnection> connectionFactory, string tableName)
        {
            var query = $"SELECT * FROM {tableName}";
            using (var connection = connectionFactory())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                DataTable schema;
                using (var reader = command.ExecuteReader())
                {
                    schema = reader.GetSchemaTable();
                }

                foreach (DataRow row in schema.Rows)
                {
                    yield return new DbColumnInfo
                    {
                        Name = row["ColumnName"].ToString(),
                        DbType = row["DataType"].ToString()
                    };
                }
            }
        }

        public class DbColumnInfo
        {
            public string Name { get; set; }
            public string DbType { get; set; }
        }
    }
}