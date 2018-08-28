using System.Threading.Tasks;

namespace MassTransit.Gateway.Gateways
{
    public interface IMessageGateway
    {
        Task Start();
        Task Stop();
    }
}