using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using GreenPipes;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using NUnit.Framework;

namespace Bus.Tests.MassTransit.AzureServiceBus
{
    [TestFixture]
    public class AzureServiceBusTests : AzureServiceBusTestBase
    {
        [Test]
        public async Task Can_publish_and_receive_message()
        {
            var semaphore = new SemaphoreSlim(0);

            var bus = global::MassTransit.Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var host = sbc.Host(ConnectionString, configurator =>
                {
                    configurator.OperationTimeout = TimeSpan.FromSeconds(5);
                });

                sbc.AutoDeleteOnIdle = 5.Minutes();

                sbc.ReceiveEndpoint(host, nameof(Can_publish_and_receive_message), ep =>
                {
                    ep.UseRetry(configurator => configurator.None());

                    ep.Handler((MessageHandler<TestMessage>)(context =>
                    {
                        context.Message.Text.Should().Be("Hi");
                        semaphore.Release();
                        return Task.CompletedTask;
                    }));
                });
            });

            await bus.StartAsync();

            await bus.Publish(new TestMessage { Text = "Hi" });

            (await semaphore.WaitAsync(1.Seconds())).Should().BeTrue();

            await bus.StopAsync();
        }

        [Test]
        public async Task Can_publish_and_receive_message_in_different_hosts()
        {
            var bus = global::MassTransit.Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                sbc.AutoDeleteOnIdle = 5.Minutes();

                var host = sbc.Host(ConnectionString, configurator =>
                {
                    configurator.OperationTimeout = TimeSpan.FromSeconds(5);
                });

            });

            var semaphore = new SemaphoreSlim(0);

            var bus2 = global::MassTransit.Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                sbc.AutoDeleteOnIdle = 5.Minutes();

                var host = sbc.Host(ConnectionString, configurator =>
                {
                    configurator.OperationTimeout = TimeSpan.FromSeconds(5);
                });

                sbc.ReceiveEndpoint(host, nameof(Can_publish_and_receive_message_in_different_hosts), ep =>
                {
                    ep.Handler((MessageHandler<TestMessage>)(context =>
                    {
                        context.Message.Text.Should().Be("Hi");
                        semaphore.Release();
                        return Task.CompletedTask;
                    }));
                });
            });

            await bus.StartAsync();
            await bus2.StartAsync();

            await bus.Publish(new TestMessage { Text = "Hi" });
            
            (await semaphore.WaitAsync(1.Seconds())).Should().BeTrue();

            await bus.StopAsync();
            await bus2.StopAsync();
        }
    }
}