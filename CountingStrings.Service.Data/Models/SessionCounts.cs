using System;

namespace CountingStrings.Service.Data.Models
{
    public class SessionCounts
    {
        public Guid Id { get; set; }
        public int NumOpen { get; set; }
        public int NumClose { get; set; }
    }
}
