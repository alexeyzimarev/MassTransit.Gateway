using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GatewayTests.Messages;
using MassTransit.Testing;
using Newtonsoft.Json;
using Serilog;
using Shouldly;
using Xunit;

namespace MassTransit.Gateway.SqlServer.Tests
{
    public class JsonTable_Specs : IAsyncLifetime, IClassFixture<JsonTableDatabaseFixure>
    {
        [Fact]
        public void Should_start()
        {
        }

        [Theory, AutoData]
        public async Task Consume_one_message(TestMessage message)
        {
            await _dbFixture.InsertRows(new[] { CreateRow(message) });

            var consumed = _handler.Consumed.Select(x => true).ToArray();
            consumed.Length.ShouldBe(1);
            IsSame(message, consumed.First().Context.Message).ShouldBeTrue();
        }

        [Theory, AutoData]
        public async Task Consume_multiple_messages(IEnumerable<TestMessage> messages)
        {
            var msgs = messages.ToList();
            await _dbFixture.InsertRows(msgs.Select(CreateRow));

            var consumed = _handler.Consumed.Select(x => true).ToArray();
            consumed.Length.ShouldBe(msgs.Count);
            foreach (var message in msgs)
            {
                var receivedMessage = consumed.FirstOrDefault(x => x.Context.Message.IntValue == message.IntValue);
                receivedMessage.ShouldNotBeNull();
                IsSame(message, receivedMessage.Context.Message).ShouldBeTrue();
            }
        }

        private static bool IsSame(TestMessage message1, TestMessage message2)
            => message1.DateValue.ToString(CultureInfo.InvariantCulture)
               == message2.DateValue.ToString(CultureInfo.InvariantCulture)
               && message1.IntValue == message2.IntValue
               && message1.StringValue == message2.StringValue;

        private static JsonTableDatabaseFixure.JsonTableRow CreateRow(TestMessage message)
            => new JsonTableDatabaseFixure.JsonTableRow
            {
                MessageType = "GatewayTests.Messages.TestMessage",
                Payload = JsonConvert.SerializeObject(message)
            };

        private readonly JsonTableDatabaseFixure _dbFixture;
        private InMemoryTestHarness _bus;
        private HandlerTestHarness<TestMessage> _handler;

        public JsonTable_Specs(JsonTableDatabaseFixure dbFixture)
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
                cfg.AddJsonQueueTableSqlServerGateway(_dbFixture.TableName,
                    _dbFixture.GetConnection,
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