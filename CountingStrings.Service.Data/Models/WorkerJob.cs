using System;

namespace CountingStrings.Service.Data.Models
{
    public class WorkerJob
    {
        public Guid Id { get; set; }

        public int ProcessId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
