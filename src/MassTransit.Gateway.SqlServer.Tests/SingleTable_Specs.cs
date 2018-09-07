using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GatewayTests.Messages;
using MassTransit.Testing;
using Serilog;
using Shouldly;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class SingleTable_Specs : IAsyncLifetime, IClassFixture<SingleTableDatabaseFixure>
    {
        [Fact]
        public void Should_start()
        {
        }

        [Theory, AutoData]
        public async Task Consume_one_message(SingleTableDatabaseFixure.TableRow row)
        {
            await _dbFixture.InsertRows(new[] {row});

            var consumed = _handler.Consumed.Select(x => true).ToArray();
            consumed.Length.ShouldBe(1);
            IsSame(row, consumed.First().Context.Message).ShouldBeTrue();
        }

        [Theory, AutoData]
        public async Task Consume_multiple_messages(IEnumerable<SingleTableDatabaseFixure.TableRow> rows)
        {
            var rowsList = rows.ToList();
            await _dbFixture.InsertRows(rowsList);

            var consumed = _handler.Consumed.Select(x => true).ToArray();
            consumed.Length.ShouldBe(rowsList.Count);
            foreach (var row in rowsList)
            {
                var message = consumed.FirstOrDefault(x => x.Context.Message.IntValue == row.IntValue);
                message.ShouldNotBeNull();
                IsSame(row, message.Context.Message).ShouldBeTrue();
            }
        }

        private static bool IsSame(SingleTableDatabaseFixure.TableRow row, TestMessage message)
            => row.DateValue.ToString(CultureInfo.InvariantCulture)
               == message.DateValue.ToString(CultureInfo.InvariantCulture)
               && row.IntValue == message.IntValue
               && row.StringValue == message.StringValue;

        private readonly SingleTableDatabaseFixure _dbFixture;
        private InMemoryTestHarness _bus;
        private HandlerTestHarness<TestMessage> _handler;

        public SingleTable_Specs(SingleTableDatabaseFixure dbFixture)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger()
                .ForContext<SingleTable_Specs>();

            _dbFixture = dbFixture;
        }

        public async Task InitializeAsync()
        {
            _bus = new InMemoryTestHarness {TestTimeout = TimeSpan.FromSeconds(2)};
            _handler = _bus.Handler<TestMessage>();

            _bus.OnConfigureBus += cfg =>
            {
                cfg.UseSerilog();
                cfg.AddMessageTableSqlServerGateway(_dbFixture.TableName,
                    _dbFixture.GetConnection,
                    "GatewayTests.Messages.TestMessage",
                    TimeSpan.FromMilliseconds(100));
            };

            await _bus.Start();
        }

        public async Task DisposeAsync()
        {
            await _bus.Stop();
            using (var connection = _dbFixture.GetConnection())
            using (var command = connection.CreateCommand())
            {
                await connection.OpenAsync();
                command.CommandText = "TRUNCATE TABLE " + _dbFixture.TableName;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}