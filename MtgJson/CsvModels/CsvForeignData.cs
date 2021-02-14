using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvForeignData
    {
        public string flavorText { get; set; } = "";

        [Name("id")]

        public int Id { get; set; }

        [Name("uuid")]
        public Guid CardId { get; set; } = Guid.Empty;

        public string language { get; set; } = "";
        public string multiverseid { get; set; } = "";
        public string name { get; set; } = "";
        public string text { get; set; } = "";
        public string type { get; set; } = "";
    }
}