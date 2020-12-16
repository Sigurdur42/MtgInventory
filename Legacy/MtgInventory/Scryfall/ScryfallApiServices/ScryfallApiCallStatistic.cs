using System;

namespace ScryfallApiServices
{
    public class ScryfallApiCallStatistic : IScryfallApiCallStatistic
    {
        public Guid Id { get; set; }

        public DateTime Today { get; set; } = DateTime.Now.Date;
        public int ScryfallCountToday { get; set; }
        public int ScryfallCountTotal { get; set; }
    }
}