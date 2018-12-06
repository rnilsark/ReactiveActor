using System;
using Bus.Abstractions;

namespace Actor1.IntegrationEvents
{
    public abstract class IntegrationEvent : IMessage
    {
        public Guid MessageId { get; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public string TypeName { get; set; }
    }
}