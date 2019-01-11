using System;
using System.Runtime.Serialization;

namespace Actor1.IntegrationEvents
{
    [DataContract]
    public class CounterDecreasedEvent : IntegrationEvent, ICounterEvent
    {
        [DataMember]
        public Guid CounterId { get; set; }
    }
}