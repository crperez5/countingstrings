using System;
using System.Collections.Generic;
using NServiceBus;

namespace CountingStrings.API.Contract
{
    public class SubmitWords : IMessage
    {
        public Guid SessionId { get; set; }        
        public List<string> Words { get; set; }
        public DateTime DateModified { get; set; }
    }
}
