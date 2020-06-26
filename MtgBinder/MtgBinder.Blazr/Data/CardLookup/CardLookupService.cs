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

        public Task<LookupResult> Lookup(CardLookupData data)
        {
            var cards = _database.LookupCards(data.Lookup, data.Mode);

            return Task.FromResult(new LookupResult()
            {
                Cards = cards.Select(c => new LookupCard(c)).ToArray()
            });
        }
    }
}