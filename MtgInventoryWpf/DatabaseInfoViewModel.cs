using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MtgDatabase;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class DatabaseInfoViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly IAutoAupdateMtgDatabaseService _autoAupdateMtgDatabaseService;

        public DatabaseSummary? DatabaseSummary { get; set; }
        public string LastUpdate { get; set; }
        private string _noUpdateYet = "Never";

        public DatabaseInfoViewModel(
            IMtgDatabaseService mtgDatabaseService,
            IAutoAupdateMtgDatabaseService autoAupdateMtgDatabaseService)
        {
            _autoAupdateMtgDatabaseService = autoAupdateMtgDatabaseService;
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

        private void UpdateDatabaseStatistics()
        {
            DatabaseSummary = _mtgDatabaseService.GetDatabaseSummary();
            var updated = DatabaseSummary?.LastUpdated ?? DateTime.MinValue;
            LastUpdate = updated == DateTime.MinValue ? _noUpdateYet : updated.ToShortDateString();
        }
    }
}
