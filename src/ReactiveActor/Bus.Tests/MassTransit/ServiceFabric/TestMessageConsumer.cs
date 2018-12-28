using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;

namespace Bus.Tests.MassTransit.ServiceFabric
{
    public class TestMessageConsumer : IConsumer<TestMessage>
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Action<TestMessage> _onMessage;
        
        public TestMessageConsumer(SemaphoreSlim semaphore, Action<TestMessage> onMessage)
        {
            _semaphore = semaphore;
            _onMessage = onMessage;
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            _semaphore.Release();
            _onMessage(context.Message);
            return Task.CompletedTask;
        }
    }
}