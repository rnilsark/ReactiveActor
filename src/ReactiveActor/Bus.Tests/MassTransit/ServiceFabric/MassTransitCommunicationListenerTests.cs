using System.Threading;
using System.Threading.Tasks;
using Bus.MassTransit.ServiceFabric;
using FluentAssertions;
using FluentAssertions.Extensions;
using MassTransit;
using NUnit.Framework;

namespace Bus.Tests.MassTransit.ServiceFabric
{
    [TestFixture]
    public class MassTransitCommunicationListenerTests
    {
        [Test]
        public async Task Can_publish_and_receive_after_opening_communication_listener()
        {
            var semaphore = new SemaphoreSlim(0);

            var busControl = global::MassTransit.Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("mass_transit", ep =>
                {   
                    ep.Consumer(() => new TestMessageConsumer(semaphore, message => message.Text.Should().Be("This is a test.")));
                });
            });

            var listener = new MassTransitCommunicationListener(busControl);

            await listener.OpenAsync(CancellationToken.None);

            await busControl.Publish(new TestMessage {Text = "This is a test."});

            (await semaphore.WaitAsync(1.Seconds())).Should().BeTrue();
        }
    }
}