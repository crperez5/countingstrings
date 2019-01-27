using System;

namespace CountingStrings.Service.Data.Models
{
    public class SessionCount
    {
        public Guid Id { get; set; }
        public int NumOpen { get; set; }
        public int NumClose { get; set; }
    }
}
