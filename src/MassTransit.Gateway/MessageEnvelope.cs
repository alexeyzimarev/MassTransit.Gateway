using System;

namespace MassTransit.SqlGateway
{
    public class MessageEnvelope
    {
        public MessageEnvelope(object message, Type type)
        {
            Message = message;
            Type = type;
        }

        public object Message { get; set; }
        public Type Type { get; set; }
    }
}