using System;
using NServiceBus;

namespace CountingStrings.API.Contract
{
    public class OpenSession : IMessage
    {
        public Guid SessionId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
