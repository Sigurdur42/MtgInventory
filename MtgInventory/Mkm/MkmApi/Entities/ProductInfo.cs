using CsvHelper.Configuration.Attributes;

namespace MkmApi
{
    public class ProductInfo
    {
        [Name("idProduct")]
        public string Id { get; set; }

        [Name("Name")]
        public string Name { get; set; }

        [Name("Category ID")]
        public string CategoryId { get; set; }

        [Name("Category")]
        public string Category { get; set; }

        [Name("Expansion ID")]
        public string ExpansionId { get; set; }

        [Name("Metacard ID")]
        public string MetacardId { get; set; }

        [Name("Date Added")]
        public string DateAdded { get; set; }
    }
}