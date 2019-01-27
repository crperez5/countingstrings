using System;

namespace CountingStrings.API.Data.Models
{
    public class WordFrequency
    {
        public string Word { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
