using System;

namespace MtgJson.Database
{
    public class DbPriceMeta
    {
        public Guid Id { get; set; } = Guid.Empty;

        public DateTime Date { get; set; } = DateTime.MinValue;

        public string Version { get; set; } = "";
    }
}
