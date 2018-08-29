using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Gateway.MessageFactories;
using MassTransit.Gateway.SqlServer.Database;
using MassTransit.Gateway.SqlServer.Exceptions;

namespace MassTransit.Gateway.SqlServer.MessageFactories
{
    public class QueueJsonTableMessageFactory : IMessageFactory
    {
        private readonly string _tableName;
        private readonly Func<SqlConnection> _connectionFactory;

        public QueueJsonTableMessageFactory(string tableName, Func<SqlConnection> connectionFactory)
        {
            _tableName = tableName;
            _connectionFactory = connectionFactory;
        }

        public async Task Initialize()
        {
            var schema = (await DbSchemaReader.ReadTableSchema(_connectionFactory, _tableName).ConfigureAwait(false)).ToArray();

            ExpectColumnType(schema, "Timestamp", typeof(DateTime));
            ExpectColumnType(schema, "MessageType", typeof(string));
            ExpectColumnType(schema, "Payload", typeof(string));
        }

        public MessageEnvelope CreateMessage(DataRow row)
        {
            throw new System.NotImplementedException();
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
    }

}