using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Gateway.Dynamics;
using MassTransit.Gateway.MessageFactories;
using MassTransit.Gateway.SqlServer.Logging;

namespace MassTransit.Gateway.SqlServer.MessageFactories
{
    public class SingleTableMessageFactory : IMessageFactory
    {
        private static readonly ILog Log = LogProvider.For<QueueJsonTableMessageFactory>();

        private readonly Func<SqlConnection> _connectionFactory;
        private readonly string _tableName;
        private readonly string _type;
        private Type _messageType;
        private PropertyDefinition[] _properties;

        public SingleTableMessageFactory(string tableName, Func<SqlConnection> connectionFactory, string type)
        {
            _connectionFactory = connectionFactory;
            _tableName = tableName;
            _type = type;
        }

        public async Task Initialize()
        {
            Log.Debug("Initializing single table queue gateway");

            var schema = await DbSchemaReader.ReadTableSchema(_connectionFactory, _tableName).ConfigureAwait(false);
            _properties = schema.Where(x => x.Name.ToLower() != "rownumber").Select(FromDbColumnInfo).ToArray();
            _messageType = DynamicTypeBuilder.BuildMessageType(new MessageTypeDefinition(_type, _properties));

            Log.Debug("Initialization complete");
        }

        public MessageEnvelope CreateMessage(DataRow row)
        {
            var properties = _properties.Select((x, i) => new PropertyValue(x.Name, row[x.Name]));
            var message = MessageBuilder.CreateMessage(_messageType, properties);
            return new MessageEnvelope(message, _messageType);
        }

        private static PropertyDefinition FromDbColumnInfo(DbSchemaReader.DbColumnInfo dbColumnInfo)
            => new PropertyDefinition(dbColumnInfo.Name, Type.GetType(dbColumnInfo.DbType));
    }
}