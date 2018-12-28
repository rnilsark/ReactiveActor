using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using MassTransit;
using NUnit.Framework;

namespace Bus.Tests.MassTransit.Testing
{
    [TestFixture]
    public class InMemoryTests
    {
        [Test]
        public async Task Can_publish_and_receive_message()
        {
            var semaphore = new SemaphoreSlim(1);

            var bus = global::MassTransit.Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("mass_transit", ep =>
                {   
                    ep.Handler<TestMessage>(context =>
                    {
                        context.Message.Text.Should().Be("Hi!");
                        semaphore.Release();
                        return Task.CompletedTask;
                    });
                });
            });

            await bus.StartAsync(); // This is important!

            await bus.Publish(new TestMessage{Text = "Hi"});

            await Task.Delay(1.Milliseconds());

            (await semaphore.WaitAsync(1.Seconds())).Should().BeTrue();

            await bus.StopAsync();
        }
    }
}
