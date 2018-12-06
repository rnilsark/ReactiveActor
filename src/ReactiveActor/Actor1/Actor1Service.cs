using System;
using System.Fabric;
using System.Threading.Tasks;
using Actor1.IntegrationEvents;
using Bus.Abstractions;
using Bus.AzureBlobStorage;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class Actor1Service : ActorService
    {
        private readonly IBus _integrationBus;

        public Actor1Service(StatefulServiceContext context, ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null, ActorServiceSettings settings = null, 
            IBus bus = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            var blobStorageConnectionString = context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                .Settings
                .Sections["BusConfig"]
                .Parameters["BlobStorageConnectionString"].Value;

            _integrationBus = bus ?? IntegrationBus.Connect(blobStorageConnectionString);
        }

        internal async Task Publish(IntegrationEvent integrationEvent)
        {
            await _integrationBus.Publish(integrationEvent);
            ActorEventSource.Current.Message($"Published {integrationEvent.MessageId}");
        }
    }
}