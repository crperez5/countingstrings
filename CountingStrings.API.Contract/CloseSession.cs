using System;
using NServiceBus;

namespace CountingStrings.API.Contract
{
    public class CloseSession : IMessage
    {
        public Guid SessionId { get; set; }
    }
}
