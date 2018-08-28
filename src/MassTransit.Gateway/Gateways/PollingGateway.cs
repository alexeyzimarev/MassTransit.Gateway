using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransit.Gateway.Gateways
{
    public class PollingGateway : IMessageGateway
    {
        private readonly IPublishEndpoint _bus;
        private readonly IMessagePoller _poller;
        private IDisposable _subscription;
        private IObservable<MessageEnvelope> _observable;
        private CancellationTokenSource _cancellationTokenSource;

        public PollingGateway(IPublishEndpoint bus, IMessagePoller poller)
            : this(bus, poller, TimeSpan.FromSeconds(1))
        {
        }

        public PollingGateway(IPublishEndpoint bus, IMessagePoller poller, TimeSpan period)
        {
            _bus = bus;
            _poller = poller;
            _cancellationTokenSource = new CancellationTokenSource();
            _observable = ObservableEx
                .Create<MessageEnvelope>(Poll, _cancellationTokenSource.Token)
                .Concat(Observable.Empty<MessageEnvelope>().Delay(period))
                .Repeat();
        }

        private async Task Poll(Action<MessageEnvelope> yield, CancellationToken token)
        {
            while (true)
            {
                var envelope = await _poller.Poll(token).ConfigureAwait(false);
                if (envelope == null) return;

                yield(envelope);
            }
        }

        public async Task Start()
        {
            await _poller.Initialize();
            _subscription = _observable.Subscribe(async m =>
                await _bus.Publish(m.Message, m.Type, _cancellationTokenSource.Token).ConfigureAwait(false));
        }

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            _subscription?.Dispose();
            return Task.CompletedTask;
        }
    }
}