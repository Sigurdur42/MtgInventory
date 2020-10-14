using System;
using MkmApi.Entities;

namespace MtgInventory.Service.Models
{
    public class MkmAdditionalCardInfo
    {
        public Guid Id { get; set; }

        public string MkmId { get; set; } = "";

        public string MkmWebSite { get; set; } = "";
        public string MkmImage { get; set; } = "";

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(MkmWebSite)
                   && !string.IsNullOrWhiteSpace(MkmImage);
        }

        public void UpdateFromProduct(Product product)
        {
            MkmId = product.IdProduct;
            MkmImage = product.Image;
            MkmWebSite = "http://" + product.WebSite;
        }
    }
}