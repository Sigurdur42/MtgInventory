using System.Collections.Generic;

namespace MtgJson.JsonModels
{
    public class JsonCardPrice
    {
        public string Id { get; set; } = "";
        public IList<JsonCardPriceItem> Items { get; set; } = new List<JsonCardPriceItem>();
    }
}
