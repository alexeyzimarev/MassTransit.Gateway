using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit.Gateway.Dynamics;
using Newtonsoft.Json.Linq;

namespace MassTransit.Gateway.Json
{
    public static class JsonTypeProvider
    {
        public static MessageEnvelope CreateMessage(string className, string messageJson)
        {
            if (className.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(className));
            if (messageJson.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(messageJson));

            var jObject = JObject.Parse(messageJson);

            var messageType = MessageTypeProvider.TryGetType(className)
                ?? BuildType(className, jObject);
            var properties = GetValues(jObject);

            return new MessageEnvelope(
                MessageBuilder.CreateMessage(messageType, properties),
                messageType);
        }

        private static IEnumerable<PropertyValue> GetValues(JToken jObject) =>
            jObject.Children()
                .Where(x => x.Type == JTokenType.Property)
                .Select(x => (JProperty) x)
                .Select(x => new PropertyValue(x.Name, ((JValue) x.Value).Value));

        private static Type BuildType(string className, JToken jObject)
        {
            var properties = jObject.Children()
                .Where(x => x.Type == JTokenType.Property)
                .Select(x => (JProperty) x)
                .Select(GetPropertyDefinition);
            var definition = new MessageTypeDefinition(className, properties.ToArray());
            return MessageTypeProvider.BuildMessageType(definition);
        }

        private static PropertyDefinition GetPropertyDefinition(JProperty j)
        {
            var typeCode = ((JValue) j.Value).Type;
            Type type;
            switch (typeCode)
            {
                case JTokenType.Integer:
                    type = typeof(long);
                    break;
                case JTokenType.Float:
                    type = typeof(float);
                    break;
                case JTokenType.String:
                    type = typeof(string);
                    break;
                case JTokenType.Boolean:
                    type = typeof(bool);
                    break;
                case JTokenType.Date:
                    type = typeof(DateTime);
                    break;
                case JTokenType.Bytes:
                    type = typeof(byte[]);
                    break;
                case JTokenType.Guid:
                    type = typeof(Guid);
                    break;
                case JTokenType.Uri:
                    type = typeof(Uri);
                    break;
                case JTokenType.TimeSpan:
                    type = typeof(TimeSpan);
                    break;
                default:
                    throw new NotImplementedByDesignException($"JSON type {typeCode.ToString()} is not supported");
            }
            return new PropertyDefinition(j.Name, type);
        }
    }
}