using Microsoft.Extensions.Logging;
using MtgDatabase;
using ScryfallApiServices;

namespace ScryfallApiConsole
{
    public class ApiAction
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

        public int RunAction(ApiOptions options)
        {
            var task = _mtgDatabaseService.RefreshLocalDatabaseAsync();
            task.GetAwaiter().GetResult();
            _logger.LogInformation("Done creating database.");
            return -1;
        }
    }
}