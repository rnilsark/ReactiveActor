using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class Actor1 : Actor, IActor1
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
            await _actorService.Publish(new IntegrationEvent{Name = "CountUpdatedEvent", Id = this.GetActorId().GetGuidId()}); //Not transactional, this message can be lost.
            ActorEventSource.Current.ActorMessage(this, "Count Updated");
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return StateManager.TryAddStateAsync("count", 0);
        }
    }
}