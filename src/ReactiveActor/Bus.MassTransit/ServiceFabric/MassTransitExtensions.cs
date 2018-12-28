using System;
using System.Fabric;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;

namespace Bus.MassTransit.ServiceFabric
{
    public static class MassTransitExtensions
    {
        public static void ReceiveEndpoint(this IInMemoryBusFactoryConfigurator @this, 
            ServiceContext serviceContext,  
            ServicePartitionInformation partitionInformation,
            Action<IInMemoryReceiveEndpointConfigurator> configureEndpoint)
        {
            @this.ReceiveEndpoint(UniqueQueueName(serviceContext, partitionInformation), configureEndpoint);
        }

        public static void ReceiveEndpoint(this IServiceBusBusFactoryConfigurator @this, IServiceBusHost host,
            ServiceContext serviceContext,
            ServicePartitionInformation partitionInformation,
            Action<IServiceBusReceiveEndpointConfigurator> configureEndpoint)
        {
            @this.ReceiveEndpoint(host, UniqueQueueName(serviceContext, partitionInformation), configureEndpoint);
        }

        private static string UniqueQueueName(ServiceContext context, ServicePartitionInformation partitionInformation)
        {
            string partitionKey;
            switch (partitionInformation)
            {
                case Int64RangePartitionInformation int64PartitionInfo:
                    partitionKey = int64PartitionInfo.LowKey.ToString(); // TODO: Figure something out that looks better.
                    break;
                case NamedPartitionInformation namedPartitionInfo:
                    partitionKey = namedPartitionInfo.Name;
                    break;
                case SingletonPartitionInformation _:
                    partitionKey = "Singleton";
                    break;
                default:
                    throw new ArgumentException("Unknown partition kind");
            }

            var serviceFullName = context.ServiceName.AbsolutePath.Substring(1).Replace('/', '_'); // TODO: Consider configuration instead.
            return $"{serviceFullName}_{partitionKey}";
        }
    }
}