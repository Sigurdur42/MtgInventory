using System.IO;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services.Sets;

namespace MtgBinders.Domain.Services
{
    internal class MtgSetService
    {
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly string _configurationFileName;
        private readonly ILogger _logger;
        private SetServiceConfiguration _configuration;

        public MtgSetService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MtgSetService>();
            _configurationSerializer = configurationSerializer;
            _configurationFileName = Path.Combine(configurationProvider.AppDataFolder, "SetConfiguration.json");
        }

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");
            _configuration = _configurationSerializer.Deserialize<SetServiceConfiguration>(_configurationFileName);

            
            // TODO: Read set data from database
        }
    }
}