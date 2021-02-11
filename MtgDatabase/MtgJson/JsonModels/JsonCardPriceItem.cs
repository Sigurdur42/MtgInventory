using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.MtgJson.JsonModels
{
    public class JsonCardPriceItem
    {
        public string Type { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Seller { get; set; } = "";
        public string PaperOrOnline { get; set; } = "";

        public string BuylistOrRetail { get; set; } = "";
        public string FoilOrNormal { get; set; } = "";
        public string Date { get; set; } = "";
        public double Price { get; set; }
    }
}
