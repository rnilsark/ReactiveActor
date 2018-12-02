using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class Actor1Service : ActorService
    {
        private readonly Bus _bus;

        public Actor1Service(StatefulServiceContext context, ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {

            var connectionString = context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                .Settings
                .Sections["BusConfig"]
                .Parameters["BlobStorageConnectionString"].Value;

            _bus = Bus.Connect(connectionString);
        }

        internal async Task Publish(IntegrationEvent integrationEvent)
        {
            await _bus.Publish(integrationEvent);
            ActorEventSource.Current.Message($"Published {integrationEvent.EventId}");
        }
    }
}