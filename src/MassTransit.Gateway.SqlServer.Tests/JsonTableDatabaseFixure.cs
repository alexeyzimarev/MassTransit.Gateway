using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Gateway.SqlServer.MessageFactories;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class JsonTableDatabaseFixure : IAsyncLifetime
    {
        private const string ConnectionString = @"Server=localhost;Database=master;User=sa;Password=Password123;";
        private readonly string _databaseName;
        private readonly string _insertStatement;
        private QueueJsonTableMessageFactory.ColumnNames _columnNames;

        public string TableName { get; }

        public JsonTableDatabaseFixure()
        {
            _columnNames = new QueueJsonTableMessageFactory.ColumnNames();
            _databaseName = Guid.NewGuid().ToString().Replace("-", "");
            TableName = $"[{_databaseName}].[dbo].[JsonTable]";
            _insertStatement =
                $"INSERT INTO {TableName} VALUES (@MessageType, @Timestamp, @Payload);";
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
{_columnNames.MessageType} varchar(200),
{_columnNames.Timestamp} datetime,
{_columnNames.Payload} text
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

        internal async Task InsertRows(IEnumerable<JsonTableRow> rows)
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

        private async Task InsertRow(SqlConnection connection, JsonTableRow row)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = _insertStatement;
                command.Parameters.AddRange(Parameters());
                command.Parameters[0].Value = row.MessageType;
                command.Parameters[1].Value = row.Payload;
                command.Parameters[2].Value = row.DateTime;
                await command.ExecuteNonQueryAsync();
            }
        }

        public SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        private static SqlParameter[] Parameters() =>
        new [] {
            new SqlParameter("@MessageType", SqlDbType.VarChar),
            new SqlParameter("@Payload", SqlDbType.Text),
            new SqlParameter("@Timestamp", SqlDbType.DateTime),
        };

        public class JsonTableRow
        {
            public string MessageType { get; set; }
            public string Payload { get; set; }
            public DateTime DateTime { get; set; } = DateTime.Now;
        }
    }
}