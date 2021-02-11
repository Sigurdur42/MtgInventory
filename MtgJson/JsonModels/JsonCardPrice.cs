using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.MtgJson.JsonModels
{
    public class JsonCardPrice
    {
        public string Id { get; set; } = "";
        public IList<JsonCardPriceItem> Items { get; set; } = new List<JsonCardPriceItem>();
    }
}
