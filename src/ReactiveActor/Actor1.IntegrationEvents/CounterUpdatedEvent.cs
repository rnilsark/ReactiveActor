using System;

namespace Actor1.IntegrationEvents
{
    public class CounterUpdatedEvent : IntegrationEvent
    {
        public Guid CounterId { get; set; }
    }
}
