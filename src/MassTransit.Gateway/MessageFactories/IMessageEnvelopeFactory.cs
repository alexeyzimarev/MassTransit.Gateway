namespace MassTransit.Gateway.MessageFactories
{
    public interface IMessageEnvelopeFactory
    {
        MessageEnvelope CreateMessage(string className, string messageJson);
    }
}
