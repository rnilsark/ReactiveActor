using System.Threading.Tasks;
using MassTransit;

namespace Web1
{
    internal class NoOpConsumer<T> : IConsumer<T> where T : class
    {
        public Task Consume(ConsumeContext<T> context)
        {
            return Task.CompletedTask;
        }
    }
}