using System;
using System.Linq;
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
        private const string OutboxStateName = "__outbox";
        private const string OutboxReminderName = "__outbox";

        private readonly Outbox<IntegrationEvent> _outbox;

        public Actor1(Actor1Service actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _outbox = new Outbox<IntegrationEvent>(this, actorService.Bus, OutboxStateName);
        }

        Task<int> IActor1.GetCountAsync(CancellationToken cancellationToken)
        {
            return StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        public async Task SetCountAsync(SetCountCommand command, CancellationToken cancellationToken)
        {
            var integrationEvent = new CounterUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CounterId = this.GetActorId().GetGuidId()
            };
            
            await StateManager.AddOrUpdateStateAsync("count", command.Count, (key, value) => command.Count > value ? command.Count : value, cancellationToken);
            
            await _outbox.Add(integrationEvent, cancellationToken);
            
            ActorEventSource.Current.ActorMessage(this, nameof(CounterUpdatedEvent));
        }
        
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, nameof(OnActivateAsync));
            return StateManager.TryAddStateAsync("count", 0);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            // TODO: Handle receive and move retry logic to Outbox
            if (reminderName == OutboxReminderName)
            {
                var attempt = BitConverter.ToInt32(state);

                try
                {
                    await _outbox.Publish();
                    ActorEventSource.Current.Message($"Published events.");
                }
                catch (Exception)
                {
                    ActorEventSource.Current.ActorMessage(this, $"Failed {nameof(_outbox.Publish)} attempt {attempt}." );

                    if (attempt > 3)
                    {
                        throw;
                    }

                    var retryTimeWithBackOff = TimeSpan.FromSeconds((1+attempt) * 2);
                    await RegisterReminderAsync(OutboxReminderName, BitConverter.GetBytes(++attempt), retryTimeWithBackOff, RemindMe.Never);
                    ActorEventSource.Current.ActorMessage(this, $"Retrying in {retryTimeWithBackOff} seconds." );
                }
            }
        }

        public new Task RegisterReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterReminderAsync(reminderName, state, dueTime, period);
        }
    }
}