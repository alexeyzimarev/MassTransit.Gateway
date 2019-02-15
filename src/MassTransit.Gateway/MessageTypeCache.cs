using System;
using System.Collections.Concurrent;

namespace MassTransit.Gateway
{
    public static class MessageTypeCache
    {
        public static void AddTypeDefinition(Type type, string typeName)
        {
            typeName = typeName ?? type.FullName;

            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException($"Invalid typename {typeName} for {type}");

            Cache[typeName] = type;
        }

        public static Type TryGetType(string className) =>
            Cache.ContainsKey(className) ? Cache[className] : null;


        private static readonly ConcurrentDictionary<string, Type> Cache =
            new ConcurrentDictionary<string, Type>();
    }
}
