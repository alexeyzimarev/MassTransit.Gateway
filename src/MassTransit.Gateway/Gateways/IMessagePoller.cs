using System.Threading;
using System.Threading.Tasks;

namespace MassTransit.Gateway.Gateways
{
    public interface IMessagePoller
    {
        Task Initialize();
        Task<MessageEnvelope> Poll(CancellationToken cancellationToken);
    }
}