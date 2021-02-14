using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvMeta
    {
        [Name("date")]
        public string Date { get; set; } = "";

        [Name("version")]
        public string Version { get; set; } = "";
    }
}