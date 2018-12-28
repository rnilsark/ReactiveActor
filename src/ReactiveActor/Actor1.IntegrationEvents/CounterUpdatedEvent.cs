using System;
using System.Runtime.Serialization;

namespace Actor1.IntegrationEvents
{
    [DataContract]
    public class CounterUpdatedEvent : IntegrationEvent
    {
        [DataMember]
        public Guid CounterId { get; set; }
    }
}
