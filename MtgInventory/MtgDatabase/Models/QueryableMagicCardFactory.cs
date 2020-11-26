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
            
            var result = new QueryableMagicCard()
            {
                Name = card.Name,
                TypeLine = card.TypeLine,
                ReprintInfos = CalculateReprints(allCards),
                Legalities = CalculateLegalities(allCards),
            };

            return result;
        }

        public ReprintInfo[] CalculateReprints(ScryfallCard[] cards)
        {
            return cards.Select(c => new ReprintInfo()
            {
                Rarity = c.Rarity.ToRarity(c.TypeLine),
                SetCode = c.Set,
                CollectorNumber = c.CollectorNumber,
            }).ToArray();
        }

        public Legality[] CalculateLegalities(ScryfallCard[] cards)
        {
            var legalities = cards
                .SelectMany(c => c.Legalities)
                .OrderBy(l=>l.Key)
                .ToArray();
            
            return legalities.Select(c => new Legality()
                {
                    Format = c.Key.ToSanctionedFormat(),
                    IsLegal = c.Value.ToLegalityState(),
                })
                .ToArray();
        }
    }
}