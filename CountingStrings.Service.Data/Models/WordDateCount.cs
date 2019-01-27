using System;

namespace CountingStrings.Service.Data.Models
{
    public class WordDateCount
    {
        public Guid Id { get; set; }
        public string Word { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
