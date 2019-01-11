using System;
using System.Threading;
using System.Threading.Tasks;
using Bus.Abstractions;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public class ReminderOutbox<T> : IOutbox<T> where T: class, IMessage
    {
        private readonly IMessageProducer _actor;
        private readonly IBackOffStrategy _backOffStrategy;
        private readonly string _stateName;
        private readonly string _reminderName;
        private readonly TimeSpan _dueTime;
        private readonly Outbox<T> _outbox;
        
        public ReminderOutbox(IMessageProducer actor, IIntegrationBus bus, IBackOffStrategy backOffStrategy = null, string stateName = null, string reminderName = "__outbox", TimeSpan dueTime = default(TimeSpan))
        {
            _actor = actor;
            _backOffStrategy = backOffStrategy ?? new FibonacciBackOffStrategy();
            _stateName = stateName;
            _reminderName = reminderName;
            _dueTime = dueTime;
            _outbox = new Outbox<T>(actor.StateManager, bus, stateName);
        }

        public async Task HandleReminderAsync(string reminderName, byte[] state, int maxAttempts)
        {
            if (reminderName == _reminderName)
            {
                var attempt = BitConverter.ToInt32(state, 0);

                try
                {
                    await _outbox.DispatchAsync();
                }
                catch (Exception)
                {
                    if (attempt > maxAttempts)
                    {
                        throw;
                    }

                    var dueTime = _backOffStrategy.GetDue(attempt);
                    await _actor.RegisterReminderAsync(_reminderName, BitConverter.GetBytes(++attempt), dueTime, RemindMe.Never);
                }
            }
        }

        private async Task RegisterReminderAsync()
        {
            await _actor.RegisterReminderAsync(_stateName, BitConverter.GetBytes(0), _dueTime, RemindMe.Never);
        }

        public Task Add(T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _outbox.Add(message, cancellationToken);
        }
    }
}