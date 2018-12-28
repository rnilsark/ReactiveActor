using System.Threading.Tasks;
using Bus.Abstractions;
using MassTransit;

namespace Bus.MassTransit
{
    public class IntegrationBus : IIntegrationBus
    {
        private readonly IBus _bus;

        public IntegrationBus(IBus bus)
        {
            _bus = bus;
        }

        public Task Publish<T>(T message) where T : class, IEvent
        {
            return _bus.Publish(message, context => Configure(context));
        }

        private static void Configure<T>(PublishContext<T> context) where T : class, IEvent
        {
            //TODO: CorrelationId
        }
    }
}