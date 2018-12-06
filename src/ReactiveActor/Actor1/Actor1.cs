using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Actor1.IntegrationEvents;
using Actor1.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Actor1 : Actor, IActor1, IRemindable
    {
        private readonly Actor1Service _actorService;

        public Actor1(Actor1Service actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
        }

        Task<int> IActor1.GetCountAsync(CancellationToken cancellationToken)
        {
            return StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        async Task IActor1.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            await StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
            var integrationEvent = new CounterUpdatedEvent{TypeName = nameof(CounterUpdatedEvent), EventId = Guid.NewGuid(), CounterId = this.GetActorId().GetGuidId()};
            await StateManager.AddOrUpdateStateAsync("outbox", new IntegrationEvent[] {integrationEvent}, (key, value) => value.Union(new[]{integrationEvent}).ToArray(), cancellationToken);
            ActorEventSource.Current.ActorMessage(this, nameof(CounterUpdatedEvent));
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, nameof(OnActivateAsync));
            return StateManager.TryAddStateAsync("count", 0);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var outbox = await StateManager.GetStateAsync<IntegrationEvent[]>("outbox", CancellationToken.None);
            foreach (var message in outbox)
            {
                await _actorService.Publish(message);
            }
            await StateManager.RemoveStateAsync("outbox", CancellationToken.None);
        }
    }
}