using System;

namespace Bus.Abstractions
{
    public interface IMessage
    {
        Guid MessageId { get; } // For de-duplication
    }
}
