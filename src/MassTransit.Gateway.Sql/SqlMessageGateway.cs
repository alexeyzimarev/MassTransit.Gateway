using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Gateway.MessageFactories;
using MassTransit.Gateway.Sql.MessageFactories;
using MassTransit.SqlGateway;

namespace MassTransit.Gateway.Sql
{
    public class SqlMessageGateway : ISqlMessageGatewayConfigurator
    {
        private IBus _bus;
        private ConnectionFactory _connectionFactory;
        private readonly List<TableGateway> _tables;

        private SqlMessageGateway(IBus bus, ConnectionFactory connectionFactory)
        {
            _bus = bus;
            _connectionFactory = connectionFactory;
            _tables = new List<TableGateway>();
        }

        public static ISqlMessageGatewayConfigurator Create(IBus bus, ConnectionFactory connectionFactory)
            => new SqlMessageGateway(bus, connectionFactory);

        public ISqlMessageGatewayConfigurator AddMessageTable(string tableName, TimeSpan pollingInterval = default)
        {
            _tables.Add(
                new TableGateway(tableName, _connectionFactory, new SingleTableMessageFactory(),
                    pollingInterval == default ? TimeSpan.FromSeconds(1) : pollingInterval));
            return this;
        }

        public ISqlMessageGatewayConfigurator AddTable(string tableName, IMessageFactory messageFactory,
            TimeSpan pollingInterval = default)
        {
            _tables.Add(new TableGateway(tableName, _connectionFactory, messageFactory,
                    pollingInterval == default ? TimeSpan.FromSeconds(1) : pollingInterval));
            return this;
        }

        public SqlMessageGateway Build()
        {
            return this;
        }

        public Task Start() => Task.WhenAll(_tables.Select(StartTable));

        private async Task StartTable(TableGateway table)
        {
//            var subscriber =
        }

        public delegate IDbConnection ConnectionFactory();
    }
}