using System;

namespace MassTransit.Gateway.SqlServer.Exceptions
{
    public class QueueSchemaException : Exception
    {
        public QueueSchemaException(string message) : base(message) { }
    }
}