using System;

namespace ScryfallApiServices
{
    public interface IScryfallApiCallStatistic
    {
        Guid Id { get; set; }

        DateTime Today { get; set; }
        int ScryfallCountToday { get; set; }
        int ScryfallCountTotal { get; set; }
    }
}