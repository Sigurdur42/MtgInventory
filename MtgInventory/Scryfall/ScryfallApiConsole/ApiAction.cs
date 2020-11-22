using Microsoft.Extensions.Logging;

namespace ScryfallApiConsole
{
    public class ApiAction
    {
        private readonly ILogger<ApiAction> _logger;


        public ApiAction(ILogger<ApiAction> logger)
        {
            _logger = logger;
        }

        public int RunAction(ApiOptions options)
        {
            _logger.LogInformation($"inside my action");
            return -1;
        }
    }
}