using System;
using System.Fabric;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;

namespace Bus.MassTransit.ServiceFabric
{
    public static class MassTransitExtensions
    {
        public static void ReceiveEndpoint(this IInMemoryBusFactoryConfigurator @this, ServiceContext context,
            Action<IInMemoryReceiveEndpointConfigurator> configureEndpoint)
        {
            @this.ReceiveEndpoint(UniqueQueueName(context), configureEndpoint);
        }

        public static void ReceiveEndpoint(this IServiceBusBusFactoryConfigurator @this, IServiceBusHost host,
            ServiceContext serviceContext,
            Action<IServiceBusReceiveEndpointConfigurator> configureEndpoint)
        {
            @this.ReceiveEndpoint(host, UniqueQueueName(serviceContext), configureEndpoint);
        }

        private static string UniqueQueueName(ServiceContext context)
        {
            return $"{context.ServiceName.AbsolutePath.Substring(1).Replace('/', '_')}_{context.PartitionId}";
        }
    }
}