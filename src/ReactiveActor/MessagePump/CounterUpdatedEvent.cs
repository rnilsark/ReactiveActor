using System;

namespace MessagePump
{
    // TODO: Copied but should be shared as a nuget package. 
    public class CounterUpdatedEvent
    {
        public Guid CounterId { get; set; }
        public Guid MessageId { get; set; }
        public Guid EventId { get; set; }
        public string SemanticEventName { get; set; }
    }
}
