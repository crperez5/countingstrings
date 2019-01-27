using System;

namespace CountingStrings.Service.Data.Models
{
    public class SessionWord
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public string Word { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
