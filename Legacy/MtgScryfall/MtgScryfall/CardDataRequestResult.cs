using MtgScryfall.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall
{
    public class CardDataRequestResult
    {
        public CardData[] CardData { get; set; }
        public bool HasMoreData { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public int TotalCards { get; set; }
    }
}