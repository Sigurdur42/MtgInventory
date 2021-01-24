using System;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using ScryfallApiServices;

namespace ScryfallApiConsole
{
    public class ApiAction : IProgress<int>
    {
        private readonly ILogger<ApiAction> _logger;
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly IScryfallService _scryfallService;

        public ApiAction(
            ILogger<ApiAction> logger,
            IScryfallService scryfallService,
            IMtgDatabaseService mtgDatabaseService)
        {
            _logger = logger;
            _scryfallService = scryfallService;
            _mtgDatabaseService = mtgDatabaseService;
        }

        public void Report(int value) => Console.WriteLine($"Database progress: {value}%");

        public int RunAction(ApiOptions options)
        {
            var task = _mtgDatabaseService.RefreshLocalDatabaseAsync(this);
            task.GetAwaiter().GetResult();
            _logger.LogInformation("Done creating database.");
            return -1;
        }
    }
}