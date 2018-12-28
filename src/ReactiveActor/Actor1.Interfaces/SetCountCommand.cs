using System;
using System.Runtime.Serialization;

namespace Actor1.Interfaces
{
    [DataContract]
    public class SetCountCommand
    {
        [DataMember]
        public Guid CommandId { get; private set; } = Guid.NewGuid();
        [DataMember]
        public int Count { get; set; }
    }
}