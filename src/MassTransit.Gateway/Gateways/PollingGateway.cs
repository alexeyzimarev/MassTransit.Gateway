using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Gateway.Logging;

namespace MassTransit.Gateway.Gateways
{
    public class PollingGateway : IMessageGateway
    {
        private static readonly ILog Log = LogProvider.For<PollingGateway>();

        private readonly IPublishEndpoint _bus;
        private readonly IMessagePoller _poller;
        private IDisposable _subscription;
        private readonly IObservable<MessageEnvelope> _observable;
        private readonly CancellationTokenSource _cancellationTokenSource;

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
                MessageEnvelope envelope;
                try
                {
                    envelope = await _poller.Poll(token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error occured while polling the queue", e);
                    envelope = null;
                }

                if (envelope == null) return;

                yield(envelope);
            }
        }

        public async Task Start()
        {
            Log.Debug("Initializer the queue poller");

            await _poller.Initialize();
            _subscription = _observable.Subscribe(async m =>
                await _bus.Publish(m.Message, m.Type, _cancellationTokenSource.Token).ConfigureAwait(false));

            Log.Debug("Polling gateway started");
        }

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            _subscription?.Dispose();
            return Task.CompletedTask;
        }
    }
}