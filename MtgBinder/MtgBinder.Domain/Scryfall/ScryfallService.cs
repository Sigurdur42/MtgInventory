using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Tools;
using MtgDomain;
using ScryfallApi.Client;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Scryfall
{
    public class ScryfallService : IScryfallService
    {
        private readonly ScryfallApiClient _apiClient;
        private readonly IAsyncProgressNotifier _progressNotifier;
        private readonly GoodCiticenAutoSleep _autoSleep = new GoodCiticenAutoSleep();

        public ScryfallService(
            ScryfallApiClient apiClient,
            IAsyncProgressNotifier progressNotifier)
        {
            _apiClient = apiClient;
            _progressNotifier = progressNotifier;
        }

        public IEnumerable<SetInfo> RetrieveSets()
        {
            _autoSleep.AutoSleep();

            var sets = _apiClient.Sets.Get().Result;
            return sets.Data.Select(s => new SetInfo()
            {
                CardCount = s.card_count,
                Code = s.Code,
                Name = s.Name,
                IsDigital = s.IsDigital,
                ReleaseDate = new SetReleaseDate(s.ReleaseDate)
            });
        }

        public ScryfallCardData[] RetrieveCardsForSetCode(string setCode)
        {
            using var logger = new ActionLogger(nameof(ScryfallService), nameof(RetrieveCardsForSetCode));

            var query = $"e:{setCode}";
            return InternalSearch(logger, query, SearchOptions.RollupMode.Prints);
        }

        public ScryfallCardData[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode)
        {
            using var logger = new ActionLogger(nameof(ScryfallService), nameof(RetrieveCardsByCardName));

            var query = $"{cardName}";
            return InternalSearch(logger, query, rollupMode);
        }

        internal ScryfallCardData[] InternalSearch(
            ActionLogger logger,
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

            var result = new List<ScryfallCardData>();
            do
            {
                _autoSleep.AutoSleep();
                cards = _apiClient.Cards.Search(lookupPattern, page, searchOptions).Result;

                if (page == 1 && cards.HasMore)
                {
                    // Initialize progress
                    var pageCount = cards.TotalCards / cards.Data.Count;
                    _progressNotifier.Start(logger.Prefix, pageCount);
                }

                _progressNotifier.NextStep(logger.Prefix);
                ++page;

                foreach (var card in cards.Data)
                {
                    result.Add(new ScryfallCardData()
                    {
                        Card = card.ToCardInfo(),
                        Price = card.ToCardPrice()
                    });
                }

                logger.Information($"Retrieved {result.Count} cards of {cards.TotalCards}");

                Thread.Sleep(100);
            } while (cards.HasMore);

            _progressNotifier.Finish(logger.Prefix);

            return result.ToArray();
        }
    }
}