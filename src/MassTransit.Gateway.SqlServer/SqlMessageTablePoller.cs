using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Gateway.Gateways;
using MassTransit.Gateway.MessageFactories;

namespace MassTransit.Gateway.Sql
{
    public class SqlServerMessageTablePoller : IMessagePoller
    {
        private string _commandText;
        private Func<SqlConnection> _connectionFactory;
        private readonly IMessageFactory _messageFactory;

        internal SqlServerMessageTablePoller(string tableName,
            Func<SqlConnection> connectionFactory,
            IMessageFactory messageFactory)
        {
            _commandText = string.Format(ReadAndDelete, tableName);
            _connectionFactory = connectionFactory;
            _messageFactory = messageFactory;
        }

        public Task Initialize() => _messageFactory.Initialize();

        public async Task<MessageEnvelope> Poll(CancellationToken cancellationToken)
        {
            var table = new DataTable();
            using (var connection = _connectionFactory())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (var command = new SqlCommand(_commandText, connection))
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!reader.HasRows) return null;

                    table.Load(reader);
                }
            }

            return _messageFactory.CreateMessage(table.Rows[0]);
        }

        private const string ReadAndDelete = @"
WITH data AS (SELECT TOP(1) * FROM {0} WITH (UPDLOCK, READPAST, ROWLOCK) ORDER BY RowVersion)
DELETE FROM data OUTPUT DELETED.*";
    }
}