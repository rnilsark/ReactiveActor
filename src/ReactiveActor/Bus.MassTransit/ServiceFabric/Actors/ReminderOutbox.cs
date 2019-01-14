using System;
using System.Threading;
using System.Threading.Tasks;
using Bus.Abstractions;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public class ReminderOutbox<T> : IOutbox<T> where T: class, IMessage
    {
        private readonly IMessageProducer _producer;
        private readonly IBackOffStrategy _backOffStrategy;
        private readonly string _reminderName;
        private readonly Outbox<T> _outbox;
        
        public ReminderOutbox(IMessageProducer producer, IIntegrationBus bus, IBackOffStrategy backOffStrategy = null, string stateName = null, string reminderName = null)
        {
            _producer = producer;
            _backOffStrategy = backOffStrategy ?? new FibonacciBackOffStrategy();
            _reminderName = reminderName ?? "__outbox";
            _outbox = new Outbox<T>(producer.StateManager, bus, stateName);
        }

        public async Task HandleReminderAsync(string reminderName, byte[] state, int maxAttempts)
        {
            if (reminderName == _reminderName)
            {
                var attempt = BitConverter.ToInt32(state, 0);

                try
                {
                    await DispatchAsync();
                }
                catch (Exception)
                {
                    if (attempt > maxAttempts)
                    {
                        throw;
                    }

                    await RegisterReminderAsync(++attempt);
                }
            }
        }

        private async Task RegisterReminderAsync(int attempt)
        {
            var dueTime = _backOffStrategy.GetDue(attempt);
            await _producer.RegisterReminderAsync(_reminderName, BitConverter.GetBytes(attempt), dueTime, Period.Never);
        }

        public async Task AddAsync(T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _outbox.AddAsync(message, cancellationToken);
            await RegisterReminderAsync(1);
        }

        public Task DispatchAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _outbox.DispatchAsync(cancellationToken);
        }
    }
}