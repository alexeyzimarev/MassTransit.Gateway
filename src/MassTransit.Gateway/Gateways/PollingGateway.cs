using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MassTransit.Gateway.Gateways
{
    public class PollingGateway : IMessageGateway, IDisposable
    {
        private readonly Func<Task<IEnumerable<MessageEnvelope>>> _poller;
        private IDisposable _subscription;

        public PollingGateway(IPublishEndpoint bus, Func<Task<IEnumerable<MessageEnvelope>>> poller)
            : this(bus, poller, TimeSpan.FromSeconds(1))
        {
        }

        public PollingGateway(IPublishEndpoint bus, Func<Task<IEnumerable<MessageEnvelope>>> poller, TimeSpan period)
        {
            _poller = poller;

            _subscription = ObservableEx
                .Create<MessageEnvelope>(Poll)
                .Concat(Observable.Empty<MessageEnvelope>().Delay(period))
                .Repeat()
                .Subscribe(async m => await bus.Publish(m.Message, m.Type));
        }

        private async Task Poll(Action<MessageEnvelope> yield)
        {
            while (true)
            {
                var page = (await _poller()).ToList();
                if (!page.Any()) return;

                foreach (var messageEnvelope in page)
                {
                    yield(messageEnvelope);
                }
            }
        }

        public void Dispose() => _subscription?.Dispose();
    }
}