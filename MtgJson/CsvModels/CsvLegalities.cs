using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvLegalities
    {
        [Name("uuid")]
        public Guid CardId { get; set; } = Guid.Empty;

        public string format { get; set; } = "";
        [Name("id")]

        public int Id { get; set; }
        public string status { get; set; } = "";
    }
}