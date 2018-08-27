using System.Data;
using MassTransit.Gateway.MessageFactories;
using MassTransit.SqlGateway;

namespace MassTransit.Gateway.Sql.MessageFactories
{
    public class SingleTableMessageFactory : IMessageFactory
    {
        public MessageEnvelope CreateMessage(DataRow row)
        {
            throw new System.NotImplementedException();
        }
    }
}