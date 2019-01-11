using System;
using Bus.Abstractions;

namespace Actor1.IntegrationEvents
{
    public interface ICounterEvent : IEvent
    {
        Guid CounterId { get; }
    }
}