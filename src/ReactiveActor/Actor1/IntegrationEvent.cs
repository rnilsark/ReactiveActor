using System;

namespace Actor1
{
    internal class IntegrationEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}