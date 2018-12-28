using System.Threading;
using System.Threading.Tasks;
using Actor1.IntegrationEvents;
using Actor1.Interfaces;
using MassTransit;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class CounterUpdatedEventConsumer : IConsumer<CounterUpdatedEvent>
    {
        private readonly IActorStateProvider _stateProvider;

        public CounterUpdatedEventConsumer(IActorStateProvider stateProvider)
        {
            _stateProvider = stateProvider;
        }

        public async Task Consume(ConsumeContext<CounterUpdatedEvent> context)
        {
            ActorEventSource.Current.Message($"{nameof(Actor1)}: Received {nameof(CounterUpdatedEvent)} with id {context.Message.MessageId}.");
   
            // TODO: Partition aware routing.
            var actorId = new ActorId(context.Message.CounterId);
            if (await _stateProvider.ContainsStateAsync(actorId, "count"))
            {
                var proxy = ActorProxy.Create<IActor1>(actorId);
                var counter = await proxy.GetCountAsync(CancellationToken.None);

                ActorEventSource.Current.Message($"{nameof(Actor1)}: Current counter value is {counter}.");
            }
            else
            {
                ActorEventSource.Current.Message($"{nameof(Actor1)}: Unknown counter {actorId}. Likely to exist on another partition.");
            }
        }
    }
}