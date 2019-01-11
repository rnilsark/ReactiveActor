using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public interface IMessageProducer : IRemindable
    {
        IActorStateManager StateManager { get; }
        Task RegisterReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period);
    }
}