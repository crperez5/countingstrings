using System;
using NServiceBus;

namespace CountingStrings.API.Contract
{
    public class LogRequest : IMessage
    {
        public Guid Id { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
