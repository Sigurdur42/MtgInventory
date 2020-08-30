using MkmApi;

namespace MtgInventory.Service.Models
{
    public class MkmProductInfo
    {
        public MkmProductInfo()
        {
        }

        public MkmProductInfo(ProductInfo info)
        {
            Id = info.Id;
            Name = info.Name;
            CategoryId = info.CategoryId;
            Category = info.Category;
            ExpansionId = info.ExpansionId;
            MetacardId = info.MetacardId;
            DateAdded = info.DateAdded;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }

        public string Category { get; set; }

        public string ExpansionId { get; set; }

        public string MetacardId { get; set; }

        public string DateAdded { get; set; }

        public string ExpansionName { get; set; }

        public string ExpansionCode { get; set; }
    }
}