using System;
using MassTransit.Gateway.MessageFactories;

namespace MassTransit.Gateway.Sql
{
    internal class TableGateway
    {
        private string _tableName;
        private SqlMessageGateway.ConnectionFactory _connectionFactory;
        private readonly IMessageFactory _messageFactory;

        internal TableGateway(string tableName,
            SqlMessageGateway.ConnectionFactory connectionFactory,
            IMessageFactory messageFactory, TimeSpan timeSpan)
        {
            _tableName = tableName;
            _connectionFactory = connectionFactory;
            _messageFactory = messageFactory;
        }
    }
}