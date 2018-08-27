using MassTransit.Gateway.Gateways;

namespace MassTransit.Gateway
{
    public static class BusExtensions
    {
        public static void AddGateway(this IBus bus, IMessageGateway gateway)
        {
        }
    }
}