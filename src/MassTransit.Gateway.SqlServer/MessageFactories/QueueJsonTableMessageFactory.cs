using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MassTransit.Gateway.Json;
using MassTransit.Gateway.MessageFactories;
using MassTransit.Gateway.SqlServer.Exceptions;
using MassTransit.Gateway.SqlServer.Logging;

namespace MassTransit.Gateway.SqlServer.MessageFactories
{
    public class QueueJsonTableMessageFactory : IMessageFactory
    {
        private readonly string _tableName;
        private readonly Func<SqlConnection> _connectionFactory;
        private readonly ColumnNames _columnNames;

        private static readonly ILog Log = LogProvider.For<QueueJsonTableMessageFactory>();

        public QueueJsonTableMessageFactory(string tableName, Func<SqlConnection> connectionFactory) :
            this(tableName, connectionFactory, new ColumnNames())
        {
        }

        public QueueJsonTableMessageFactory(string tableName, Func<SqlConnection> connectionFactory,
            ColumnNames columnNames)
        {
            _tableName = tableName;
            _connectionFactory = connectionFactory;
            _columnNames = columnNames;
        }

        public async Task Initialize()
        {
            Log.Debug("Initializing JSON table queue gateway");

            var schema = (await DbSchemaReader.ReadTableSchema(_connectionFactory, _tableName).ConfigureAwait(false)).ToArray();

            ExpectColumnType(schema, _columnNames.Timestamp, typeof(DateTime));
            ExpectColumnType(schema, _columnNames.MessageType, typeof(string));
            ExpectColumnType(schema, _columnNames.Payload, typeof(string));

            Log.Debug("Initialization complete");
        }

        public MessageEnvelope CreateMessage(DataRow row)
        {
            try
            {
                var messageClassName = row[_columnNames.MessageType].ToString();
                if (!messageClassName.Contains("."))
                    throw new InvalidOperationException($"Message type name {messageClassName} must include a namespace");

                var payload = row[_columnNames.Payload].ToString();
                return JsonTypeProvider.CreateMessage(messageClassName, payload);
            }
            catch (Exception e)
            {
                Log.ErrorException("Error occured when creating a message from the data row", e);
                return null;
            }
        }

        private void ExpectColumnType(IEnumerable<DbSchemaReader.DbColumnInfo> schema, string name, Type type)
        {
            var columnInfo = schema.FirstOrDefault(x => x.Name == name);
            if (columnInfo == null)
                throw new QueueSchemaException($"Column {name} not found in the table {_tableName}");

            var columnType = Type.GetType(columnInfo.DbType);
            if (columnType == null)
                throw new QueueSchemaException($"Column {name} has an unknown type {columnInfo.DbType}");

            if (columnType != type)
                throw new QueueSchemaException($"Column {name} is expected to be of type {type.Name} but has the type {columnType}");
        }

        public class ColumnNames
        {
            public string MessageType { get; set; }= "MessageType";
            public string Timestamp { get; set; } = "Timestamp";
            public string Payload { get; set; } = "Payload";
        }
    }
}