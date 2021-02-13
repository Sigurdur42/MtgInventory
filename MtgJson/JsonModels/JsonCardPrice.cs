using System;
using System.Collections.Generic;

namespace MtgJson.JsonModels
{
    public class JsonCardPrice
    {
        public Guid Id { get; set; } = Guid.Empty;
        public List<JsonCardPriceItem> Items { get; set; } = new List<JsonCardPriceItem>();
    }
}
