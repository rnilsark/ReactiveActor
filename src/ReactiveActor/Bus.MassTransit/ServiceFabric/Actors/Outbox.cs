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

        public Outbox(IActorStateManager stateManager, IIntegrationBus bus, string stateName = null)
        {
            _stateManager = stateManager;
            _bus = bus;
            _stateName = stateName ?? "__outbox";
        }

        public async Task AddAsync(T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateManager.AddOrUpdateStateAsync(_stateName, new[] {message}, (key, value) => value.Union(new[]{message}).ToArray(), cancellationToken);
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
    }
}
