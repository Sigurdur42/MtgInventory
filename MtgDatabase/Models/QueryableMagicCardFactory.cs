﻿using System;
using System.Globalization;
using MtgDatabase.Scryfall;
using ScryfallApiServices.Models;

namespace MtgDatabase.Models
{
    public class QueryableMagicCardFactory
    {
        public QueryableMagicCard Create(ScryfallCard card)
        {
            // var legalities = CalculateLegalities(allCards);
            // var reprintInfos = CalculateReprints(card);
            var result = new QueryableMagicCard
            {
                Name = card.Name,
                Language = "EN",
                LocalName = card.Name,
                TypeLine = card.TypeLine,
                Rarity = card.Rarity.ToRarity(card.TypeLine),
                // ReprintInfos = reprintInfos,
                // Legalities = legalities,
                // IsBasicLand = reprintInfos.Any(r => r.Rarity == Rarity.BasicLand),
                CollectorNumber = card.CollectorNumber,
                OracleText = card.OracleText,
                Images = CalculateImages(card),
                SetCode = card.Set,
                SetName = card.SetName,
                Usd = card.Price.Usd,
                UsdFoil = card.Price.UsdFoil,
                Eur = card.Price.Eur,
                EurFoil = card.Price.EurFoil,
                Tix = card.Price.Tix,
                UpdateDateUtc = DateTime.Now
            };

            result.UniqueId = $"{result.Name}_{result.SetCode}_{result.CollectorNumber}".ToUpperInvariant();

            result.IsBasicLand = result.Rarity == Rarity.BasicLand;
            UpdateFromTypeLine(result, card.TypeLine);
            return result;
        }

        public QueryableMagicCard Create(ScryfallJsonCard card)
        {
            // var legalities = CalculateLegalities(allCards);
            // var reprintInfos = CalculateReprints(card);
            var result = new QueryableMagicCard
            {
                Name = card.Name,
                LocalName = card.PrintedName ?? card.Name,
                Language = card.Lang,
                TypeLine = card.TypeLine,
                Rarity = card.Rarity.ToRarity(card.TypeLine),
                // ReprintInfos = reprintInfos,
                // Legalities = legalities,
                // IsBasicLand = reprintInfos.Any(r => r.Rarity == Rarity.BasicLand),
                CollectorNumber = card.CollectorNumber,
                OracleText = card.OracleText,
                Images = CalculateImages(card),
                SetCode = card.Set,
                SetName = card.SetName,
                Usd = ConvertToNullableDecimal(card.Prices.Usd),
                UsdFoil = ConvertToNullableDecimal(card.Prices.UsdFoil),
                Eur = ConvertToNullableDecimal(card.Prices.Eur),
                EurFoil = ConvertToNullableDecimal(card.Prices.EurFoil),
                Tix = ConvertToNullableDecimal(card.Prices.Tix),
                UpdateDateUtc = DateTime.Now
            };

            result.UniqueId = $"{result.Name}_{result.SetCode}_{result.CollectorNumber}_{result.Language}".ToUpperInvariant();

            result.IsBasicLand = result.Rarity == Rarity.BasicLand;
            UpdateFromTypeLine(result, card.TypeLine);
            return result;
        }

        private static decimal? ConvertToNullableDecimal(object input)
        {
            var serialized = input?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return null;
            }

            if (decimal.TryParse(serialized, NumberStyles.Any, CultureInfo.InvariantCulture, out var converted))
            {
                return converted;
            }

            return null;
        }

        public void UpdateFromTypeLine(QueryableMagicCard card, string typeLine)
        {
            card.IsCreature = typeLine.Contains("creature", StringComparison.InvariantCultureIgnoreCase);
            card.IsInstant = typeLine.Contains("instant", StringComparison.InvariantCultureIgnoreCase);
            card.IsSorcery = typeLine.Contains("sorcery", StringComparison.InvariantCultureIgnoreCase);
            card.IsArtefact = typeLine.Contains("artefact", StringComparison.InvariantCultureIgnoreCase);
            card.IsLand = typeLine.Contains("land", StringComparison.InvariantCultureIgnoreCase);
            card.IsToken = typeLine.Contains("token", StringComparison.InvariantCultureIgnoreCase);
            card.IsEmblem = typeLine.EndsWith("emblem", StringComparison.InvariantCultureIgnoreCase);
            card.IsEnchantment = typeLine.Contains("enchantment", StringComparison.InvariantCultureIgnoreCase);
            card.IsLegendary = typeLine.Contains("legendary", StringComparison.InvariantCultureIgnoreCase);
            card.IsSnow = typeLine.Contains("snow ", StringComparison.InvariantCultureIgnoreCase);
            // ReSharper disable once StringLiteralTypo
            card.IsPlaneswalker = typeLine.Contains("planeswalker", StringComparison.InvariantCultureIgnoreCase);
        }

        // public ReprintInfo[] CalculateReprints(ScryfallCard[] cards)
        // {
        //     return cards.Select(c => new ReprintInfo()
        //     {
        //         Rarity = c.Rarity.ToRarity(c.TypeLine),
        //         SetCode = c.Set,
        //         SetName = c.SetName,
        //         CollectorNumber = c.CollectorNumber,
        //         Images = CalculateImages(c),
        //     }).ToArray();
        // }

        public CardImages CalculateImages(ScryfallCard card)
        {
            var result = new CardImages();
            foreach (var image in card.Images)
            {
                switch (image.Category?.ToLowerInvariant() ?? "")
                {
                    case "normal":
                        result.Normal = image.Uri;
                        break;

                    case "small":
                        result.Small = image.Uri;
                        break;
                    case "large":
                        result.Large = image.Uri;
                        break;
                    case "png":
                        result.Png = image.Uri;
                        break;
                    case "art_crop":
                        result.ArtCrop = image.Uri;
                        break;
                    case "border_crop":
                        result.BorderCrop = image.Uri;
                        break;
                }
            }

            return result;
        }

        private CardImages CalculateImages(ScryfallJsonCard card)
        {
            var result = new CardImages
            {
                Normal = card.ImageUris?.Normal ?? "",
                Small = card.ImageUris?.Small ?? "",
                Large = card.ImageUris?.Large ?? "",
                Png = card.ImageUris?.Png ??"",
                ArtCrop = card.ImageUris?.ArtCrop ?? "",
                BorderCrop = card.ImageUris?.BorderCrop ?? ""
            };
            
            return result;
        }

        // public Legality[] CalculateLegalities(ScryfallCard card)
        // {
        //     var legalities = cards
        //         .SelectMany(c => c.Legalities)
        //         .OrderBy(l => l.Key)
        //         .ToArray();
        //
        //     return legalities.Select(c =>
        //             new Legality() {Format = c.Key.ToSanctionedFormat(), IsLegal = c.Value.ToLegalityState(),})
        //         .ToArray();
        // }
    }
}