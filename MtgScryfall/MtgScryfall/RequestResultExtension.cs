using MtgScryfall.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;

namespace MtgScryfall
{
    public static class RequestResultExtension
    {
        public static CardDataRequestResult DeserializeCardData(this RequestResult requestResult)
        {
            var result = new CardDataRequestResult
            {
                StatusCode = requestResult.StatusCode,
                Success = requestResult.Success,
            };

            var deserialized = JsonConvert.DeserializeObject<RootObject>(requestResult.JsonResult);
            result.HasMoreData = deserialized.has_more;
            result.CardData = deserialized.data.Select(c => new CardData
            {
                Name = c.name,
                SetCode = c.set,
                Rarity = c.rarity,
            }).ToArray();

            return result;
        }

        public static SetData[] DeserializeSetData(this RequestResult result)
        {
            if (!result.Success)
            {
                return new SetData[0];
            }

            var definition = new { Name = "" };

            var deserialized = JsonConvert.DeserializeObject<JsonSetDataRootObject>(result.JsonResult);
            return deserialized.data.Select(d => new SetData
            {
                SetCode = d.code,
                SetName = d.name,
                SvgUrl = d.icon_svg_uri,
                IsDigitalOnly = d.digital,
                SetType = d.set_type,
                NumberOfCards = d.card_count
            }).ToArray();
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
    }
}