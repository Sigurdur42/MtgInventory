using Microsoft.Extensions.Logging;

namespace MtgBinders.Domain.Configuration
{
    public class BinderDomainConfigurationProvider : IBinderDomainConfigurationProvider
    {
        private readonly ILogger _logger;

        public BinderDomainConfigurationProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BinderDomainConfigurationProvider>();
        }

        public string AppDataFolder { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(
            string appDataBasePath)
        {
            IsInitialized = true;
            AppDataFolder = appDataBasePath;

            _logger.LogDebug($"Init done with: AppDataFolder={AppDataFolder}");
        }
    }
}