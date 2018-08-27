using System;

namespace MassTransit.Gateway.Sql
{
    public interface ISqlMessageGatewayConfigurator
    {
        ISqlMessageGatewayConfigurator AddMessageTable(string tableName, TimeSpan pollingInterval);
        SqlMessageGateway Build();
    }
}