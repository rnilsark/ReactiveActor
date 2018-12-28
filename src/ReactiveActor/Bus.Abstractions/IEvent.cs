using System;

namespace Bus.Abstractions
{
    public interface IEvent : IMessage
    {
        Guid EventId { get; }
    }
}
