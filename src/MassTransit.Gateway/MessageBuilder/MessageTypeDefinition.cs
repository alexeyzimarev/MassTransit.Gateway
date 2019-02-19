using System;

namespace MassTransit.Gateway.MessageBuilder
{
    public class MessageTypeDefinition
    {
        public MessageTypeDefinition(string className, PropertyDefinition[] propertyDefinitions)
        {
            ClassName = className;
            PropertyDefinitions = propertyDefinitions;
        }

        public string ClassName { get;  }
        public PropertyDefinition[] PropertyDefinitions { get;  }
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