using System;
using MkmApi;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using Serilog;

namespace MtgInventory.Service
{
    public sealed class MkmPriceService
    {
        private readonly CardDatabase _cardDatabase;
        private readonly MkmRequest _mkmRequest;

        public MkmPriceService(
            CardDatabase cardDatabase,
            MkmRequest mkmRequest)
        {
            _cardDatabase = cardDatabase;
            _mkmRequest = mkmRequest;
        }
      

        ////public CardPrice AutoDownloadMkmPrice(MkmAuthenticationData authenticationData, string mkmId, Guid scryfallId)
        ////{
        ////    var latestPrice = GetLatestPrice(mkmId, scryfallId);

        ////    if (latestPrice == null
        ////        || latestPrice.UpdateDate.Value.AddDays(1) > DateTime.Now
        ////        || (!latestPrice.ScryfallEur.HasValue && latestPrice.Source == CardPriceSource.Scryfall))
        ////    {
        ////        Log.Information($"Price for mkm id {mkmId} is outdated - downloading current one");

        ////        if (!authenticationData.IsValid())
        ////        {
        ////            Log.Warning($"MKM authentication configuration is missing - cannot access MKM API.");
        ////            return latestPrice ?? new CardPrice();
        ////        }

        ////        var result = _mkmRequest.GetProductData(authenticationData, mkmId);
        ////        latestPrice = new CardPrice(result);
        ////        _cardDatabase.CardPrices.Insert(latestPrice);
        ////        _cardDatabase.EnsureCardPriceIndex();
        ////    }

        ////    return latestPrice;
        ////}

        public CardPrice GetLatestPrice(string mkmId, Guid scryfallId)
        {
            CardPrice result = null;
            var query = _cardDatabase.CardPrices.Query();

            if (mkmId != null)
            {
                result = query.Where(c => c.MkmId == mkmId).FirstOrDefault();
            }
            
            if (scryfallId != null && result == null)
            {
                result = query.Where(c => c.ScryfallId == scryfallId).FirstOrDefault();
            }

            return result;
        }
    }
}