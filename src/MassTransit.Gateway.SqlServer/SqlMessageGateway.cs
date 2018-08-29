using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MassTransit.Gateway.Gateways;
using MassTransit.Gateway.MessageFactories;
using MassTransit.Gateway.SqlServer;
using MassTransit.Gateway.SqlServer.MessageFactories;
using MassTransit.Pipeline.Observables;

namespace MassTransit.Gateway.Sql
{
    public static class BusExtensions
    {
        public static void AddSqlServerGateway(this IBusFactoryConfigurator configurator,
            string tableName,
            Func<SqlConnection> connectionFactory,
            string messageType,
            TimeSpan pollingInterval)
        {
            configurator.ConnectBusObserver(new BusObserver(tableName, connectionFactory, messageType, pollingInterval));
        }

        private class BusObserver : IBusObserver
        {
            private readonly string _tableName;
            private readonly Func<SqlConnection> _connectionFactory;
            private readonly string _messageType;
            private readonly TimeSpan _pollingInterval;
            private PollingGateway _gateway;

            public BusObserver(string tableName, Func<SqlConnection> connectionFactory, string messageType, TimeSpan pollingInterval)
            {
                _tableName = tableName;
                _connectionFactory = connectionFactory;
                _messageType = messageType;
                _pollingInterval = pollingInterval;
            }

            public Task PostCreate(IBus bus)
            {
                var poller = new SqlServerMessageTablePoller(_tableName, _connectionFactory,
                    new SingleTableMessageFactory(_tableName, _connectionFactory, _messageType));
                _gateway = new PollingGateway(bus, poller, _pollingInterval);
                return Task.CompletedTask;
            }

            public Task CreateFaulted(Exception exception) => Task.CompletedTask;

            public Task PreStart(IBus bus) => Task.CompletedTask;

            public Task PostStart(IBus bus, Task<BusReady> busReady) => _gateway.Start();

            public Task StartFaulted(IBus bus, Exception exception) => Task.CompletedTask;

            public Task PreStop(IBus bus) => _gateway.Stop();

            public Task PostStop(IBus bus) => Task.CompletedTask;

            public Task StopFaulted(IBus bus, Exception exception) => Task.CompletedTask;
        }
    }
}