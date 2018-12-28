using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bus.Abstractions;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public interface IMessageProducer : IRemindable
    {
        IActorStateManager StateManager { get; }
        Task RegisterReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period);
    }

    public class Outbox<T> where T: IMessage
    {
        private readonly IMessageProducer _actor;
        private readonly IIntegrationBus _bus;
        private readonly string _stateName;
        
        public Outbox(IMessageProducer actor, IIntegrationBus bus, string stateName)
        {
            _actor = actor;
            _bus = bus;
            _stateName = stateName;
        }

        public async Task Add(T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _actor.StateManager.AddOrUpdateStateAsync(_stateName, new T[] {message}, (key, value) => value.Union(new[]{message}).ToArray(), cancellationToken);
            await _actor.RegisterReminderAsync(_stateName, BitConverter.GetBytes(0), TimeSpan.FromMilliseconds(100), RemindMe.Never);
        }

        public async Task Publish(CancellationToken cancellationToken = default(CancellationToken))
        {
            var outbox = await _actor.StateManager.TryGetStateAsync<T[]>(_stateName, cancellationToken);

            if (!outbox.HasValue)
                return;

            foreach (var message in outbox.Value)
            {
                await _bus.Publish((dynamic) message);
            }
            await _actor.StateManager.RemoveStateAsync(_stateName, cancellationToken);
        }
    }
}
