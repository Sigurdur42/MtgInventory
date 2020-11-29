using System;
using Microsoft.Extensions.Logging;
using MtgDatabase;

namespace MtgInventoryBlazor
{
    public class MtgInventoryService
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly ILogger<MtgInventoryService> _logger;

        public MtgInventoryService(
            IMtgDatabaseService mtgDatabaseService,
            ILogger<MtgInventoryService> logger)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _logger = logger;
        }

        public string Dummy => "My hello world";

        public event EventHandler DatabaseInitialised;
        public bool IsDatabaseInitialized { get; private set; }
        public void CreateDatabase()
        {
            try
            {
                _logger.LogInformation($"Starting database init...");
                _mtgDatabaseService.CreateDatabase(false, false);
            }
            finally
            {
                _logger.LogInformation($"Finished database init...");
                IsDatabaseInitialized = true;
                DatabaseInitialised?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}