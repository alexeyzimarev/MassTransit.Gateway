using System.Data;
using MassTransit.SqlGateway;

namespace MassTransit.Gateway.MessageFactories
{
    public interface IMessageFactory
    {
        MessageEnvelope CreateMessage(DataRow row);
    }
}