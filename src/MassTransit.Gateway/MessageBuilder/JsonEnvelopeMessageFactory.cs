using System;
using Newtonsoft.Json;

namespace MassTransit.Gateway.MessageBuilder
{
    public static class JsonEnvelopeMessageFactory
    {
        public static MessageEnvelope CreateMessage(string className, string messageJson)
        {
            if (className.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(className));
            if (messageJson.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(messageJson));

            var messageType = MessageTypeCache.TryGetType(className);
            if (messageType == null)
                throw new Exception($"Type {className} was not found in cache");

            return new MessageEnvelope(
                JsonConvert.DeserializeObject(messageJson, messageType),
                messageType);
        }
    }
}