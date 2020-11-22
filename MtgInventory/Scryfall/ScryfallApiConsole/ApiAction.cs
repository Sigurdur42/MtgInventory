using Microsoft.Extensions.Logging;
using ScryfallApiServices;

namespace ScryfallApiConsole
{
    public class ApiAction
    {
        private readonly ILogger<ApiAction> _logger;
        private readonly IScryfallService _scryfallService;


        public ApiAction(
            ILogger<ApiAction> logger,
            IScryfallService scryfallService)
        {
            _logger = logger;
            _scryfallService = scryfallService;
        }

        public int RunAction(ApiOptions options)
        {
            _scryfallService.RefreshLocalMirror(options.Clear);
            return -1;
        }
    }
}