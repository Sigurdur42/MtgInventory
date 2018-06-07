using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Scryfall;
using MtgBinders.Domain.Services.Sets;
using MtgBinders.Domain.ValueObjects;
using System;
using System.IO;

namespace MtgBinders.Domain.Services
{
    internal class MtgSetService : IMtgSetService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;
        private readonly IScryfallService _scryfallService;
        private readonly string _setCacheFileName;
        private SetServiceConfiguration _configuration;
        private MtgSetRepository _setRepository;

        public MtgSetService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory,
            IScryfallService scryfallService)
        {
            _setRepository = new MtgSetRepository();
            _logger = loggerFactory.CreateLogger<MtgSetService>();
            _configurationSerializer = configurationSerializer;
            _scryfallService = scryfallService;

            _configurationFileName = Path.Combine(configurationProvider.AppDataFolder, "SetConfiguration.json");
            _setCacheFileName = Path.Combine(configurationProvider.AppDataFolder, "SetCache.json");

            _logger.LogDebug($"Configuarion details: {Environment.NewLine}- Configuration: {_configurationFileName}{Environment.NewLine}- Set Cache: {_setCacheFileName}");
        }

        public event EventHandler InitializeDone;

        public DateTime? LastUpdatedCacheAt => _configuration?.LastUpdate;
        public IMtgSetRepository SetRepository => _setRepository;

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");

            _configuration = _configurationSerializer.Deserialize<SetServiceConfiguration>(_configurationFileName);
            if (_configuration == null)
            {
                _configuration = new SetServiceConfiguration
                {
                    LastUpdate = DateTime.UtcNow,
                };
                _configurationSerializer.Serialize(_configurationFileName, _configuration);
                _logger.LogInformation($"Created initial configuration {_configurationFileName}");
            }

            var setCache = _configurationSerializer.Deserialize<MtgSetInfo[]>(_setCacheFileName);
            if (setCache == null)
            {
                _logger.LogInformation("Cannot find cached sets data. No sets are loaded.");
            }
            else
            {
                _logger.LogInformation($"Found set data last updated at {_configuration?.LastUpdate}. Loaded {setCache.Length} sets.");
                _setRepository.SetSetData(setCache);
            }

            InitializeDone?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateSetsFromScryfall()
        {
            _logger.LogDebug("Starting update from Scryfall...");

            var newSets = _scryfallService.LoadAllSets();
            _setRepository.SetSetData(newSets);

            _configurationSerializer.Serialize(_setCacheFileName, newSets);

            _configuration.LastUpdate = DateTime.UtcNow;
            _configurationSerializer.Serialize(_configurationFileName, _configuration);

            _logger.LogInformation($"Updates set data at {_configuration?.LastUpdate}. Loaded {newSets.Length} sets.");
        }
    }
}