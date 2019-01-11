using Actor1.IntegrationEvents;
using Actor1.Interfaces;
using MassTransit;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Web1
{
    internal class CounterEventConsumer : IConsumer<ICounterEvent>
    {
        public async Task Consume(ConsumeContext<ICounterEvent> context)
        {
            ServiceEventSource.Current.Message(
                $"{nameof(Web1)}: Received {nameof(ICounterEvent)} with id {context.Message.MessageId}.");

            var actorId = new ActorId(context.Message.CounterId);
            var proxy = ActorProxy.Create<IActor1>(actorId);
            var counter = await proxy.GetCountAsync(CancellationToken.None);

            ServiceEventSource.Current.Message($"{nameof(Web1)}: Current counter value is {counter}.");
        }
    }
}