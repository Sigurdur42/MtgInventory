using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvLegalities
    {
        public string format { get; set; } = "";
        [Name("uuid")]
        public Guid Id { get; set; } = Guid.Empty;

        public string status { get; set; } = "";
    }
}