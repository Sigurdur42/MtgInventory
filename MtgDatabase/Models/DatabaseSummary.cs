using System;

namespace MtgDatabase.Models
{
    public class DatabaseSummary
    {
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public int NumberOfSets { get; set; }
        public int NumberOfCards { get; set; }
        
        public int NumberOfCardsEn { get; set; }
        public int NumberOfCardsDe { get; set; }
        public int NumberOfCardsNoLanguage { get; set; }
    }
}