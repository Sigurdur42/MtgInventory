using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvSet
    {
        [Name("baseSetSize")]
        public int BaseSetSize { get; set; }

        [Name("block")] public string Block { get; set; } = "";
        public string booster { get; set; } = "";

        [Name("keyruneCode")]
        public string Code { get; set; } = "";

        public bool isFoilOnly { get; set; }
        public bool isForeignOnly { get; set; }
        public bool isNonFoilOnly { get; set; }

        [Name("isOnlineOnly")]
        public bool IsOnlineOnly { get; set; }

        public string isPartialPreview { get; set; } = "";
        public string mcmId { get; set; } = "";
        public string mcmIdExtras { get; set; } = "";
        public string mcmName { get; set; } = "";
        public string mtgoCode { get; set; } = "";
        [Name("name")] public string Name { get; set; } = "";
        public string parentCode { get; set; } = "";
        public string releaseDate { get; set; } = "";
        public string tcgplayerGroupId { get; set; } = "";
        public string totalSetSize { get; set; } = "";
        public string type { get; set; } = "";
    }
}