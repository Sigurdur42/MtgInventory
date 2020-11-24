using System;
using System.IO;
using Microsoft.Extensions.Logging;
using ScryfallApiServices;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IDisposable
    {
        void Configure(DirectoryInfo folder);
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly Database.MtgDatabase _database;
        private readonly IScryfallService _scryfallService;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IScryfallService scryfallService)
        {
            _logger = logger;
            _database = database;
            _scryfallService = scryfallService;
        }

        public void Configure(DirectoryInfo folder)
        {
            _database.Configure(folder);
        }
        public void Dispose()
        {
            _database?.ShutDown();
        }
    }
}