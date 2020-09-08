using System;

namespace MkmApi
{
    public class ApiCallStatistics : IApiCallStatistic
    {
        public Guid Id { get; set; }

        public DateTime Today { get; set; } = DateTime.Now.Date;
        public int CountToday { get; set; }
        public int CountTotal { get; set; }
    }
}