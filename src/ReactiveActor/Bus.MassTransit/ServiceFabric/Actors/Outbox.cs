using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bus.Abstractions;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public class Outbox<T> : IOutbox<T> where T: class, IMessage
    {
        private readonly IActorStateManager _stateManager;
        private readonly IIntegrationBus _bus;
        private readonly string _stateName;

        public Outbox(IActorStateManager stateManager, IIntegrationBus bus, string stateName = "__outbox")
        {
            _stateManager = stateManager;
            _bus = bus;
            _stateName = stateName;
        }

        public event Func<EventArgs, Task> Added;

        public async Task Add(T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateManager.AddOrUpdateStateAsync(_stateName, new[] {message}, (key, value) => value.Union(new[]{message}).ToArray(), cancellationToken);
            await OnAdded();
        }
        
        public async Task DispatchAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var outbox = await _stateManager.TryGetStateAsync<T[]>(_stateName, cancellationToken);

            if (!outbox.HasValue)
                return;

            foreach (var message in outbox.Value)
            {
                await _bus.Publish((dynamic)message);
            }

            await _stateManager.RemoveStateAsync(_stateName, cancellationToken);
        }

        protected virtual Task OnAdded()
        {
            return Added?.Invoke(EventArgs.Empty);
        }
    }
}
