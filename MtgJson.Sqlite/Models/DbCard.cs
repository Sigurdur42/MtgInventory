using System;
using System.Collections.Generic;
using System.Text;

namespace MtgJson.Sqlite.Models
{
    public class DbCard
    {
        public int? Id { get; set; }
        public string Name { get; set; } = "";
        public string? NameDE { get; set; }
        public string Uuid { get; set; } = "";
        public string? ScryfallId { get; set; }
        public string TypeLine { get; set; } = "";
        public string OracleText { get; set; } = "";
        public string SetCode { get; set; } = "";
        public string? CardMarketId { get; set; }
        public string CollectorNumber { get; set; } = "";

        public string Rarity { get; set; } = "";
        public string OtherFaceIds { get; set; } = "";
        public string Side { get; set; } = "";

        public string GetScryfallImageUrl()
        {
            var facePart = Side == "b"
            ? "face=back"
            : "face=front";

            return $"https://api.scryfall.com/cards/{ScryfallId}?format=image&{facePart}";
        }

        #region PriceData
        public decimal? MkmNormal { get; set; }
        public decimal? MkmFoil { get; set; }
        public decimal? TcgPlayerFoil { get; set; }
        public decimal? TcgPlayerNormal { get; set; }
        #endregion
    }
}
