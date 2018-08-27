using System.Data;

namespace MassTransit.Gateway.MessageFactories
{
    public interface IMessageFactory
    {
        MessageEnvelope CreateMessage(DataRow row);
    }
}