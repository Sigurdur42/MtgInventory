using System.Collections.Generic;
using System.Linq;

using ScryfallApi.Client;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Scryfall
{
    public class ScryfallService : IScryfallService
    {
        private readonly ScryfallApiClient _apiClient;

        private readonly GoodCiticenAutoSleep _autoSleep = new GoodCiticenAutoSleep();

        public ScryfallService(
            ScryfallApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public IEnumerable<Set> RetrieveSets()
        {
            _autoSleep.AutoSleep();

            var sets = _apiClient.Sets.Get().Result;
            var result = sets.Data.ToArray();
            foreach (var item in result)
            {
                item.Code = item.Code?.ToUpperInvariant();
            }

            return result;
        }

        public Card[] RetrieveCardsForSetCode(string setCode)
        {
            var query = $"e:{setCode}";
            return InternalSearch(query, SearchOptions.RollupMode.Prints);
        }

        public Card[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode)
        {
            var query = $"{cardName}";
            return InternalSearch(query, rollupMode);
        }

        public Card[] RetrieveCardsByCardNameAndSet(string cardName, string setCode, SearchOptions.RollupMode rollupMode)
        {
            var query = $"!'{cardName}' e:{setCode}";
            return InternalSearch(query, rollupMode);
        }

        internal Card[] InternalSearch(
            string lookupPattern,
            SearchOptions.RollupMode rollupMode)
        {
            var page = 1;
            ResultList<Card> cards;

            var searchOptions = new SearchOptions()
            {
                Mode = rollupMode,
                IncludeExtras = true,
                Direction = SearchOptions.SortDirection.Asc,
                Sort = SearchOptions.CardSort.Name
            };

            var result = new List<Card>();
            do
            {
                _autoSleep.AutoSleep();
                cards = _apiClient.Cards.Search(lookupPattern, page, searchOptions).Result;

                if (page == 1 && cards.HasMore)
                {
                    // Initialize progress
                    var pageCount = cards.TotalCards / cards.Data.Count;
                }

                ++page;

                if (cards.Data != null)
                {
                    result.AddRange(cards.Data);
                }

                // TODO: Handle errors
            } while (cards.HasMore);

            return result.ToArray();
        }

      
    }
}