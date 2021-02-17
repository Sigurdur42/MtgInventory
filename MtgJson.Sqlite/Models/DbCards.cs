using System;
using System.Collections.Generic;
using System.Text;

namespace MtgJson.Sqlite.Models
{
    public class DbCards
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? NameDE { get; set; }
        public string Uuid { get; set; } = "";
        public string? ScryfallId { get; set; }
        public string TypeLine { get; set; } = "";
        public string OracleText { get; set; } = "";
        public string SetCode { get; set; } = "";
        public string? CardMarketId { get; set; }
        public string CollectorNumber { get; set; } = "";
    }
}
