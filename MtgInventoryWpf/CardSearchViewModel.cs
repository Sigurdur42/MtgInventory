using System;
using System.Threading.Tasks;
using MtgDatabase;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class CardSearchViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;

        public CardSearchViewModel(
            IMtgDatabaseService mtgDatabaseService)
        {
            _mtgDatabaseService = mtgDatabaseService;
        }

        public string SearchToken { get; set; } = "";

        public QueryableMagicCard[] SearchResult { get; set; } = Array.Empty<QueryableMagicCard>();

        public async Task PerformSearch()
        {
            var queryData = new MtgDatabaseQueryData
            {
                Name = SearchToken,
                
            };

            SearchResult = await _mtgDatabaseService.SearchCardsAsync(queryData);
        }
    }
}
