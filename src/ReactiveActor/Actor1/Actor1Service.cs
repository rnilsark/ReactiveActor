using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using Bus.Abstractions;
using Bus.MassTransit;
using Bus.MassTransit.ServiceFabric;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Actor1
{
    internal class Actor1Service : ActorService
    {
        private readonly string _connectionString;

        internal IIntegrationBus Bus;

        public Actor1Service(StatefulServiceContext context, ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            _connectionString = context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                .Settings
                .Sections["BusConfig"]
                .Parameters["AzureServiceBusConnectionString"].Value;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var busControl = MassTransit.Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var host = sbc.Host(_connectionString, configurator =>
                {
                    configurator.OperationTimeout = TimeSpan.FromSeconds(5);
                });

                sbc.ReceiveEndpoint(
                    host: host,
                    serviceContext: Context,
                    partitionInformation: Partition.PartitionInfo,
                    configureEndpoint: configurator => configurator.Consumer(() => new CounterUpdatedEventConsumer(StateProvider)));

                sbc.AutoDeleteOnIdle = TimeSpan.FromMinutes(5);
            });
            
            Bus = new IntegrationBus(busControl);

            return base.CreateServiceReplicaListeners().Union(
                new List<ServiceReplicaListener> {new ServiceReplicaListener(context => new MassTransitCommunicationListener(busControl))});
        }
    }
}