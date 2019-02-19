namespace MassTransit.Gateway.MessageBuilder
{
    public delegate MessageEnvelope MessageEnvelopeFactory(string className, string messageJson);
}