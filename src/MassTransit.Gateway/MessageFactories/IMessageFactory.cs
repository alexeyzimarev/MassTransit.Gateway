using System.Data;
using System.Threading.Tasks;

namespace MassTransit.Gateway.MessageFactories
{
    public interface IMessageFactory
    {
        Task Initialize();
        MessageEnvelope CreateMessage(DataRow row);
    }
}