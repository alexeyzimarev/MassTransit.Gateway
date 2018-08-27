using System;

namespace MassTransit.Gateway.Dynamics
{
    public class MessageTypeDefinition
    {
        public string ClassName { get; set; }
        public PropertyDefinition[] PropertyDefinitions { get; set; }
    }

    public class PropertyDefinition
    {
        public PropertyDefinition(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public Type Type { get; set; }

    }
}