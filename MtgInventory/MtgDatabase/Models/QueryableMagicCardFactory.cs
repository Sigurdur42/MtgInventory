using System;
using System.Collections.Generic;
using System.Linq;
using ScryfallApiServices.Models;

namespace MtgDatabase.Models
{
    public class QueryableMagicCardFactory
    {
        public QueryableMagicCard Create(IEnumerable<ScryfallCard> cards)
        {
            var allCards = cards.ToArray();
            var card = allCards.First();

            var legalities = CalculateLegalities(allCards);
            var reprintInfos = CalculateReprints(allCards);
            var result = new QueryableMagicCard()
            {
                Name = card.Name,
                TypeLine = card.TypeLine,
                ReprintInfos = reprintInfos,
                Legalities = legalities,
                IsBasicLand = reprintInfos.Any(r => r.Rarity == Rarity.BasicLand),
            };

            UpdateFromTypeLine(result, card.TypeLine);
            return result;
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

        public ReprintInfo[] CalculateReprints(ScryfallCard[] cards)
        {
            return cards.Select(c => new ReprintInfo()
            {
                Rarity = c.Rarity.ToRarity(c.TypeLine), SetCode = c.Set, CollectorNumber = c.CollectorNumber, Images = CalculateImages(c),
            }).ToArray();
        }

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
                    
                    default:
                        // TODO: log this
                        break;
                }
            }


         

            return result;
        }

        public Legality[] CalculateLegalities(ScryfallCard[] cards)
        {
            var legalities = cards
                .SelectMany(c => c.Legalities)
                .OrderBy(l => l.Key)
                .ToArray();

            return legalities.Select(c => new Legality() {Format = c.Key.ToSanctionedFormat(), IsLegal = c.Value.ToLegalityState(),})
                .ToArray();
        }
    }
}