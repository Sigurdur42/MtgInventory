using System;
using MkmApi;
using MkmApi.Entities;

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

        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public int CategoryId { get; set; }

        public string Category { get; set; } = "";

        public string ExpansionId { get; set; } = "";

        public string MetacardId { get; set; } = "";

        public string DateAdded { get; set; } = "";

        public string ExpansionName { get; set; } = "";

        public string ExpansionCode { get; set; } = "";

        public DateTime? LastDetailUpdate { get; set; }

        public string MkmProductUrl { get; set; } = "";
        
        public string MkmImage { get; set; } = "";

        public void UpdateFromProduct(Product rhs)
        {
            MkmProductUrl = "https://www.cardmarket.com" + rhs.WebSite;
            LastDetailUpdate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Id} {Name} [{ExpansionCode} {ExpansionName}] [{Category}] ({MetacardId})";
        }
    }
}