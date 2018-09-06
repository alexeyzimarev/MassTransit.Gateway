using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MassTransit.Gateway.Gateways;
using MassTransit.Gateway.SqlServer.Logging;
using MassTransit.Gateway.SqlServer.MessageFactories;

// ReSharper disable UnusedMember.Global

namespace MassTransit.Gateway.SqlServer
{
    public static class BusExtensions
    {
        public static void AddMessageTableSqlServerGateway(this IBusFactoryConfigurator configurator,
            string tableName,
            Func<SqlConnection> connectionFactory,
            string messageType,
            TimeSpan pollingInterval)
        {
            var poller = new SqlServerMessageTablePoller(tableName, connectionFactory,
                new SingleTableMessageFactory(tableName, connectionFactory, messageType));
            configurator.ConnectBusObserver(new BusObserver(poller,
                pollingInterval));
        }

        public static void AddJsonQueueTableSqlServerGateway(this IBusFactoryConfigurator configurator,
            string tableName,
            Func<SqlConnection> connectionFactory,
            TimeSpan pollingInterval)
        {
            var poller = new SqlServerMessageTablePoller(tableName, connectionFactory,
                new QueueJsonTableMessageFactory(tableName, connectionFactory));
            configurator.ConnectBusObserver(new BusObserver(poller,
                pollingInterval));
        }

        private class BusObserver : IBusObserver
        {
            private static readonly ILog Log = LogProvider.For<BusObserver>();

            private readonly IMessagePoller _poller;
            private readonly TimeSpan _pollingInterval;
            private PollingGateway _gateway;

            public BusObserver(IMessagePoller poller, TimeSpan pollingInterval)
            {
                _poller = poller;
                _pollingInterval = pollingInterval;
            }

            public Task PostCreate(IBus bus)
            {
                Log.Debug("Attaching SQL Server gateway to the bus");

                _gateway = new PollingGateway(bus, _poller, _pollingInterval);

                Log.Debug("SQL Server gateway attached");

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