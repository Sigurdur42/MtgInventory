using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MtgBinders.Domain.Services
{
    internal class MtgWantsListService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;

        public MtgWantsListService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory)

        {
            _logger = loggerFactory.CreateLogger(nameof(MtgWantsListService));
            _configurationSerializer = configurationSerializer;
            _configurationFileName = Path.Combine(configurationProvider.AppDataFolder, "Wants.json");

            _logger.LogDebug($"Configuarion details: {Environment.NewLine}- Configuration: {_configurationFileName}");
        }

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");

            // Look for cached cards
            ////var stopwatch = Stopwatch.StartNew();
            ////var allCards = new List<MtgFullCard>();
            ////var cachedCardFiles = Directory.EnumerateFiles(_cardsCacheFolder, "CardCache*.json");
            ////foreach (var cachedCardFile in cachedCardFiles)
            ////{
            ////    var cards = _configurationSerializer.Deserialize<MtgFullCard[]>(cachedCardFile);
            ////    _logger.LogDebug($"Loaded {cards.Length} cards of set {cards.First().SetCode}");

            ////    allCards.AddRange(cards);
            ////}

            ////stopwatch.Stop();
            ////_logger.LogDebug($"Loading wants list took {stopwatch.Elapsed}");

            ////_cardRepository.SetCardData(allCards);
        }
    }
}