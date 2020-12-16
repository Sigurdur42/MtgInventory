using Microsoft.Extensions.Logging;
using MtgDatabase;
using ScryfallApiServices;

namespace ScryfallApiConsole
{
    public class ApiAction
    {
        private readonly ILogger<ApiAction> _logger;
        private readonly IScryfallService _scryfallService;
        private readonly IMtgDatabaseService _mtgDatabaseService;


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
            // _scryfallService.RefreshLocalMirror(options.Clear);
            
            _mtgDatabaseService.RefreshLocalDatabase(options.ClearScryfall, options.ClearMtgDatabase);
            
            _logger.LogInformation("Done creating database.");
            return -1;
        }
    }
}