using System;
using System.Threading;
using System.Threading.Tasks;
using Actor1.IntegrationEvents;
using Actor1.Interfaces;
using Bus.MassTransit.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Actor1 : Actor, IActor1, IMessageProducer, IRemindable
    {
        private readonly ReminderOutbox<ICounterEvent> _outbox;

        public Actor1(Actor1Service actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _outbox = new ReminderOutbox<ICounterEvent>(this, actorService.Bus);
        }

        Task<int> IActor1.GetCountAsync(CancellationToken cancellationToken)
        {
            return StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        public async Task SetCountAsync(SetCountCommand command, CancellationToken cancellationToken)
        {
            var count = await StateManager.TryGetStateAsync<int>("count", cancellationToken);
            var countValue = count.HasValue ? count.Value : 0;
            
            await StateManager.AddOrUpdateStateAsync("count", command.Count, (key, value) => command.Count > value ? command.Count : value, cancellationToken);

            if (command.Count > countValue)
            {
                await _outbox.Add(new CounterIncreasedEvent
                {
                    EventId = Guid.NewGuid(),
                    CounterId = this.GetActorId().GetGuidId()
                }, cancellationToken);

                ActorEventSource.Current.ActorMessage(this, nameof(CounterIncreasedEvent));
            }
            else if (command.Count < countValue)
            {
                await _outbox.Add(new CounterDecreasedEvent
                {
                    EventId = Guid.NewGuid(),
                    CounterId = this.GetActorId().GetGuidId()
                }, cancellationToken);

                ActorEventSource.Current.ActorMessage(this, nameof(CounterDecreasedEvent));
            }
            else
            {
                ActorEventSource.Current.ActorMessage(this, "No-op");
            }
        }
        
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, nameof(OnActivateAsync));
            return StateManager.TryAddStateAsync("count", 0);
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            return _outbox.HandleReminderAsync(reminderName, state, maxAttempts: 10);
        }

        public new Task RegisterReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterReminderAsync(reminderName, state, dueTime, period);
        }
    }
}