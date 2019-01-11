using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Actor1.IntegrationEvents;
using Bus.MassTransit.ServiceFabric;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Web1
{
    internal sealed class Web1 : StatelessService
    {
        private readonly string _connectionString;

        public Web1(StatelessServiceContext context)
            : base(context)
        {   _connectionString = context
            .CodePackageActivationContext
            .GetConfigurationPackageObject("Config")
            .Settings
            .Sections["BusConfig"]
            .Parameters["AzureServiceBusConnectionString"].Value;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
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
                    configureEndpoint: configurator =>
                    {
                        configurator.Consumer<CounterEventConsumer>();
                        configurator.Consumer<NoOpConsumer<CounterIncreasedEvent>>(); // This sets up subscription, the generic consumer above consumes it.
                        configurator.Consumer<NoOpConsumer<CounterDecreasedEvent>>(); // This sets up subscription, the generic consumer above consumes it.
                    });

                sbc.AutoDeleteOnIdle = TimeSpan.FromMinutes(5);
            });
            
            return new[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }), "HttpEndpoint"),
                new ServiceInstanceListener(serviceContext => new MassTransitCommunicationListener(busControl), "ServiceBus")
            };
        }
    }
}
