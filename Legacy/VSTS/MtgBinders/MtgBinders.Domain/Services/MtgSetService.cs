using System;
using System.IO;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Scryfall;
using MtgBinders.Domain.Services.Sets;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Services
{
    internal class MtgSetService : IMtgSetService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;
        private readonly IScryfallService _scryfallService;
        private readonly string _setCacheFileName;
        private readonly IMtgSetRepository _setRepository;
        private SetServiceConfiguration _configuration;

        public MtgSetService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory,
            IScryfallService scryfallService,
            IMtgSetRepository setRepository)
        {
            _setRepository = setRepository;
            _logger = loggerFactory.CreateLogger(nameof(MtgSetService));
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

            _configuration = _configurationSerializer.Deserialize<SetServiceConfiguration>(_configurationFileName, null);
            if (_configuration == null)
            {
                _configuration = new SetServiceConfiguration
                {
                    LastUpdate = DateTime.UtcNow,
                };
                _configurationSerializer.Serialize(_configurationFileName, _configuration);
                _logger.LogInformation($"Created initial configuration {_configurationFileName}");
            }

            var setCache = _configurationSerializer.Deserialize<MtgSetInfo[]>(_setCacheFileName, null);
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

        public void UpdateSetsFromScryfall(bool checkLastUpdateDate)
        {
            if (_configuration.LastUpdate.Date < DateTime.UtcNow.AddDays(-7))
            {
                _logger.LogDebug($"Skipping set update until {_configuration.LastUpdate.Date.AddDays(7)}");
                return;
            }

            _logger.LogDebug("Starting update from Scryfall...");

            var newSets = _scryfallService.LoadAllSets();
            _setRepository.SetSetData(newSets);

            WriteSetsToCache();
        }

        public void WriteSetsToCache()
        {
            _configurationSerializer.Serialize(_setCacheFileName, _setRepository.SetData);

            _configuration.LastUpdate = DateTime.UtcNow;
            _configurationSerializer.Serialize(_configurationFileName, _configuration);

            _logger.LogInformation($"Updates set data at {_configuration?.LastUpdate}. Loaded {_setRepository.SetData.Length} sets.");
        }
    }
}