using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class SingleTableDatabaseFixure : IAsyncLifetime
    {
        private const string ConnectionString = @"Server=localhost;Database=master;User=sa;Password=Password123;";
        private readonly string _databaseName;
        private readonly string _insertStatement;

        public string TableName { get; }

        public SingleTableDatabaseFixure()
        {
            _databaseName = Guid.NewGuid().ToString().Replace("-", "");
            TableName = $"[{_databaseName}].[dbo].[SomeTable]";
            _insertStatement =
                $"INSERT INTO {TableName} VALUES (@IntValue, @StringValue, @DatetimeValue);";
        }

        public async Task InitializeAsync()
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqlCommand($"CREATE DATABASE [{_databaseName}]", connection);
                await command.ExecuteNonQueryAsync();

                command.CommandText = $@"
CREATE TABLE {TableName} (
RowNumber int PRIMARY KEY IDENTITY,
IntValue int,
StringValue varchar(200),
DateValue datetime
);";
                await command.ExecuteNonQueryAsync();
                command.Dispose();
            }
        }

        public async Task DisposeAsync()
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var command = new SqlCommand($"DROP DATABASE [{_databaseName}]", connection);
                await command.ExecuteNonQueryAsync();
            }
        }

        internal async Task InsertRows(IEnumerable<TableRow> rows)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                foreach (var row in rows)
                {
                    await InsertRow(connection, row);
                }
            }
        }

        private async Task InsertRow(SqlConnection connection, TableRow row)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = _insertStatement;
                command.Parameters.AddRange(Parameters());
                command.Parameters[0].Value = row.IntValue;
                command.Parameters[1].Value = row.StringValue;
                command.Parameters[2].Value = row.DateValue;
                await command.ExecuteNonQueryAsync();
            }
        }

        public SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        private static SqlParameter[] Parameters() =>
        new [] {
            new SqlParameter("@IntValue", SqlDbType.Int),
            new SqlParameter("@StringValue", SqlDbType.VarChar),
            new SqlParameter("@DatetimeValue", SqlDbType.DateTime),
        };

        public class TableRow
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
            public DateTime DateValue { get; set; }
        }
    }
}