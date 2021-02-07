using System;
using System.Threading.Tasks;
using MtgDatabase;
using MtgDatabase.Cache;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class CardSearchViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly IImageCache _imageCache;

        public CardSearchViewModel(
            IMtgDatabaseService mtgDatabaseService,
            IImageCache imageCache)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _imageCache = imageCache;
        }

        public string SearchToken { get; set; } = "";

        public QueryableMagicCard[] SearchResult { get; set; } = Array.Empty<QueryableMagicCard>();
        public QueryableMagicCard? SearchResultSelectedItem { get; set; }

        public string? SelectedImage { get; set; }

        public async Task PerformSearch()
        {
            var queryData = new MtgDatabaseQueryData
            {
                Name = SearchToken,

            };

            SearchResult = await _mtgDatabaseService.SearchCardsAsync(queryData);
        }

        public void OnSearchResultSelectedItemChanged()
        {
            SelectedImage = SearchResultSelectedItem is QueryableMagicCard card ? _imageCache.GetCachedImage(card) : null;
        }


    }
}
