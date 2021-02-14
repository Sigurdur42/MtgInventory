using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvSet
    {
        // 	booster	code	isFoilOnly	isForeignOnly	isNonFoilOnly	isPartialPreview		mcmId	mcmIdExtras	mcmName	mtgoCode		parentCode	releaseDate	tcgplayerGroupId	totalSetSize	type
        [Name("baseSetSize")]
        public int BaseSetSize { get; set; }

        [Name("block")] public string Block { get; set; } = "";

        [Name("keyruneCode")] public string Code { get; set; } = "";

        [Name("name")] public string Name { get; set; } = "";

        [Name("isOnlineOnly")]
        public bool IsOnlineOnly { get; set; }
    }
}