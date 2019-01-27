using System;

namespace CountingStrings.API.Data.Models
{
    public class Session
    {
        public Guid Id { get; set; }

        public int Status { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
