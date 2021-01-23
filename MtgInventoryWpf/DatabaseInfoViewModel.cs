using System;
using System.Globalization;
using System.Threading;
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


        public DatabaseSummary? DatabaseSummary { get; set; }
        public string LastUpdate { get; set; }
        private string _noUpdateYet = "Never";

        public DatabaseInfoViewModel(
            IMtgDatabaseService mtgDatabaseService)
        {
            LastUpdate = _noUpdateYet;
            Task.Factory.StartNew(UpdateDatabaseStatistics);
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
