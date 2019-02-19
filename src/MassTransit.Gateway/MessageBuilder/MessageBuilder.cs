using System;
using System.Collections.Generic;
using System.Reflection;

namespace MassTransit.Gateway.MessageBuilder
{
    public static class MessageBuilder
    {
        public static object CreateMessage(Type type, IEnumerable<PropertyValue> properties)
        {
            var message = Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                type.InvokeMember(property.Name, BindingFlags.SetProperty, null, message,
                    new[] {property.Value});
            }

            return message;
        }
    }

    public struct PropertyValue
    {
        public PropertyValue(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
    }
}