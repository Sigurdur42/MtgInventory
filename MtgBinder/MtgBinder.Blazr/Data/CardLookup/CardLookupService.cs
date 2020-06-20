using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MtgBinder.Domain.Database;
using ScryfallApi.Client.Models;

namespace MtgBinder.Blazr.Data.CardLookup
{
    public class CardLookupService
    {
        private readonly ICardDatabase _database;

        public CardLookupService(ICardDatabase database)
        {
            _database = database;
        }

        public async Task<LookupResult> Lookup(CardLookupData data)
        {
            var cards = _database.LookupCards(data.Lookup, SearchOptions.RollupMode.Prints);

            return new LookupResult()
            {
                Cards = cards.Select(c=>new LookupCard(c)).ToArray()
            };
        }
    }
}
