using System;
using System.Runtime.Serialization;
using Bus.Abstractions;

namespace Actor1.IntegrationEvents
{
    [DataContract]
    [KnownType(typeof(CounterIncreasedEvent))]
    [KnownType(typeof(CounterDecreasedEvent))]
    public abstract class IntegrationEvent : IEvent
    {
        [DataMember]
        public Guid MessageId { get; private set; } = Guid.NewGuid();
        [DataMember]
        public Guid EventId { get; set; }
        [DataMember]
        public string TypeName { get; set; }
    }
}