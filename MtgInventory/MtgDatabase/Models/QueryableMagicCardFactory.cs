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
            
            // TODO: Reprints here
            // TODO: Legalities here
            
            var result = new QueryableMagicCard()
            {
                Name = card.Name,
            };

            return result;
        }
        
    }
}