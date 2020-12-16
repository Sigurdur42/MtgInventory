using System;

namespace MkmApi
{
    public interface IApiCallStatistic
    {
        Guid Id { get; set; }

        DateTime Today { get; set; }
        int CountToday { get; set; }
        int CountTotal { get; set; }
    }
}