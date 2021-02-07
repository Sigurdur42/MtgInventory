using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MtgDatabase;
using MtgDatabase.Cache;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class DatabaseInfoViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly IAutoAupdateMtgDatabaseService _autoAupdateMtgDatabaseService;
        private readonly IImageCache _imageCache;

        public DatabaseSummary? DatabaseSummary { get; set; }
        public string LastUpdate { get; set; }
        private string _noUpdateYet = "Never";

        public DatabaseInfoViewModel(
            IMtgDatabaseService mtgDatabaseService,
            IAutoAupdateMtgDatabaseService autoAupdateMtgDatabaseService,
            IImageCache imageCache)
        {
            _autoAupdateMtgDatabaseService = autoAupdateMtgDatabaseService;
            _imageCache = imageCache;
            LastUpdate = _noUpdateYet;

            autoAupdateMtgDatabaseService.UpdateFinished += (sender, e) => UpdateDatabaseStatistics();

            Task.Factory.StartNew(() =>
            {
                if (!Debugger.IsAttached
                || (_mtgDatabaseService?.GetDatabaseSummary()?.NumberOfCards ?? 0) == 0)
                {
                    _autoAupdateMtgDatabaseService.Start();
                }

                UpdateDatabaseStatistics();
            });
            _mtgDatabaseService = mtgDatabaseService;
        }

        internal void DownloadAllImages()
        {
            Task.Factory.StartNew(() =>
            {
                var allCards = _mtgDatabaseService.Cards
                ?.Query()
                ?.Where(c => c.Language == "en")
                ?.ToArray()
                ?? Array.Empty<QueryableMagicCard>();

                _imageCache.QueueForDownload(allCards);

            });
        }

        private void UpdateDatabaseStatistics()
        {
            DatabaseSummary = _mtgDatabaseService.GetDatabaseSummary();
            var updated = DatabaseSummary?.LastUpdated ?? DateTime.MinValue;
            LastUpdate = updated == DateTime.MinValue ? _noUpdateYet : updated.ToShortDateString();
        }
    }
}
