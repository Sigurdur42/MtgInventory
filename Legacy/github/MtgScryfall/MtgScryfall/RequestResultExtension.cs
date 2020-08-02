using MtgScryfall.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace MtgScryfall
{
    public static class RequestResultExtension
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
        };

        public static CardDataRequestResult DeserializeCardData(this RequestResult requestResult)
        {
            var result = new CardDataRequestResult
            {
                StatusCode = requestResult.StatusCode,
                Success = requestResult.Success,
            };

            if (!result.Success)
            {
                return result;
            }

            var deserialized = JsonConvert.DeserializeObject<RootObject>(requestResult.JsonResult, _settings);
            result.HasMoreData = deserialized.has_more;
            result.TotalCards = deserialized.total_cards;

            result.CardData = deserialized.data.Select(MapFromJson).ToArray();

            return result;
        }

        public static CardDataRequestResult DeserializeSingleResultCardData(this RequestResult requestResult)
        {
            var result = new CardDataRequestResult
            {
                StatusCode = requestResult.StatusCode,
                Success = requestResult.Success,
            };

            if (!result.Success)
            {
                return result;
            }

            var deserialized = JsonConvert.DeserializeObject<Datum>(requestResult.JsonResult, _settings);
            result.HasMoreData = false;
            result.TotalCards = 1;

            result.CardData = new[] { MapFromJson(deserialized) };

            return result;
        }

        public static SetData[] DeserializeSetData(this RequestResult result)
        {
            if (!result.Success)
            {
                return new SetData[0];
            }

            var definition = new { Name = "" };

            var deserialized = JsonConvert.DeserializeObject<JsonSetDataRootObject>(result.JsonResult, _settings);
            return deserialized.data.Select(d => new SetData
            {
                SetCode = d.code,
                SetName = d.name,
                SvgUrl = d.icon_svg_uri,
                IsDigitalOnly = d.digital,
                SetType = d.set_type,
                NumberOfCards = d.card_count,
                IsFoilOnly = d.foil_only,
                ReleaseDate = d.released_at,
                ParentSetData = d.parent_set_code,
            }).ToArray();
        }

        public static bool IsLegal(string legality)
        {
            switch (legality?.ToLowerInvariant())
            {
                case "legal":
                    return true;

                default:
                    return false;
            }
        }

        internal static RequestResult CreateResult(this HttpResponseMessage response)
        {
            var result = new RequestResult
            {
                StatusCode = (int)response.StatusCode,
                Success = response.IsSuccessStatusCode,
            };

            if (response.IsSuccessStatusCode)
            {
                result.JsonResult = response.Content.ReadAsStringAsync().Result;
            }

            return result;
        }

        private static CardData MapFromJson(Datum c)
        {
            return new CardData
            {
                UniqueId = c.id,
                Name = c.name,
                SetCode = c.set,
                Rarity = c.rarity,
                ManaCost = c.mana_cost,
                ConvertedManaCost = c.cmc,
                ImageLarge = c.image_uris?.large,
                TypeLine = c.type_line,
                OracleText = c.oracle_text,
                CollectorNumber = c.collector_number,
                IsDigitalOnly = c.digital,
                Layout = c.layout,

                IsPauperLegal = IsLegal(c.legalities?.pauper),
                IsCommanderLegal = IsLegal(c.legalities?.commander),
                IsLegacyLegal = IsLegal(c.legalities?.legacy),
                IsVintageLegal = IsLegal(c.legalities?.vintage),
                IsStandardLegal = IsLegal(c.legalities?.standard),
                IsModernLegal = IsLegal(c.legalities?.modern),
                MkmLink = c.purchase_uris?.magiccardmarket,
                ScryfallLink = c.scryfall_uri,
                GathererLink = c.related_uris?.gatherer,
                PriceUsd = c.usd,
                PriceTix = c.tix,
                PriceEur = c.eur,
            };
        }
    }
}