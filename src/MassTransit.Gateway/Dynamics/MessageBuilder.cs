using System;
using System.Reflection;

namespace MassTransit.Gateway.Dynamics
{
    public static class MessageBuilder
    {
        public static object CreateMessage(Type type, PropertyValue[] properties)
        {
            var message = Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                type.InvokeMember(property.Name, BindingFlags.SetProperty, null, message,
                    new object[] {property.Value});
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