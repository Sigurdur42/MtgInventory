﻿using System;
using MkmApi.Entities;
using ScryfallApiServices.Models;

namespace MtgInventory.Service.Models
{
    public class CardPrice
    {
        public CardPrice()
        {
        }

        public CardPrice(ScryfallCard card)
        {
            ScryfallId = card.Id;
            ScryfallUsd = card.Price?.Usd;
            ScryfallUsdFoil = card.Price?.UsdFoil;
            ScryfallEur = card.Price?.Eur;
            ScryfallEurFoil = card.Price?.EurFoil;
            ScryfallTix = card.Price?.Tix;

            UpdateDate = card.UpdateDateUtc;
            Source = CardPriceSource.Scryfall;
        }

        public CardPrice(Product result)
        {
            MkmPriceSell = result.PriceGuide.PriceSell;
            MkmPriceLow = result.PriceGuide.PriceLow;
            MkmPriceLowEx = result.PriceGuide.PriceLowEx;
            MkmPriceLowFoil = result.PriceGuide.PriceLowFoil;
            MkmPriceAverage = result.PriceGuide.PriceAverage;
            MkmPriceTrend = result.PriceGuide.PriceTrend;
            MkmPriceTrendFoil = result.PriceGuide.PriceTrendFoil;
            UpdateDate = DateTime.UtcNow;
            Source = CardPriceSource.Mkm;
        }

        public Guid Id { get; set; }

        public string MkmId { get; set; } = "";

        public Guid ScryfallId { get; set; }

        public decimal? ScryfallUsd { get; set; }

        public decimal? ScryfallUsdFoil { get; set; }

        public decimal? ScryfallEur { get; set; }

        public decimal? ScryfallEurFoil { get; set; }

        public decimal? ScryfallTix { get; set; }

        public decimal? MkmPriceSell { get; set; }

        public decimal? MkmPriceLow { get; set; }

        public decimal? MkmPriceLowEx { get; set; }

        public decimal? MkmPriceLowFoil { get; set; }

        public decimal? MkmPriceAverage { get; set; }

        public decimal? MkmPriceTrend { get; set; }

        public decimal? MkmPriceTrendFoil { get; set; }

        public DateTime? UpdateDate { get; set; }

        public CardPriceSource Source { get; set; }

        public decimal? GetMarketPrice(bool isFoil)
        {
            switch (Source)
            {
                case CardPriceSource.Scryfall:
                    return isFoil ? ScryfallEurFoil : ScryfallEur;

                default:
                    return isFoil ? MkmPriceTrendFoil : MkmPriceTrend;
            }
        }
    }
}