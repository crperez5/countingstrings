using System;

namespace CountingStrings.Service.Data.Models
{
    public class SessionWordCount
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Word { get; set; }
        public int Count { get; set; }
        public DateTime DateCreated { get; set; }
    }
}