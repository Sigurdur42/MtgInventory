using Mapster;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Scryfall;
using MtgBinders.Domain.Services.Sets;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MtgBinders.Domain.Services
{
    internal class MtgCardService : IMtgCardService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;
        private readonly IScryfallService _scryfallService;
        private readonly string _cardsCacheFolder;
        private readonly IMtgCardRepository _cardRepository;
        private CardServiceConfiguration _configuration;

        public MtgCardService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory,
            IScryfallService scryfallService,
            IMtgCardRepository cardRepository)
        {
            _cardRepository = cardRepository;
            _logger = loggerFactory.CreateLogger<MtgSetService>();
            _configurationSerializer = configurationSerializer;
            _scryfallService = scryfallService;

            _configurationFileName = Path.Combine(configurationProvider.AppDataFolder, "CardConfiguration.json");
            _cardsCacheFolder = configurationProvider.AppDataFolder;

            _logger.LogDebug($"Configuarion details: {Environment.NewLine}- Configuration: {_configurationFileName}{Environment.NewLine}");
        }

        public event EventHandler InitializeDone;

        public int NumberOfCards => _cardRepository.NumberOfCards;

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");

            _configuration = _configurationSerializer.Deserialize<CardServiceConfiguration>(_configurationFileName, null);
            if (_configuration == null)
            {
                _configuration = new CardServiceConfiguration
                {
                    LastUpdate = DateTime.UtcNow,
                };
                _configurationSerializer.Serialize(_configurationFileName, _configuration);
                _logger.LogInformation($"Created initial configuration {_configurationFileName}");
            }

            // Look for cached cards
            var stopwatch = Stopwatch.StartNew();
            var allCards = new List<MtgFullCard>();
            var cachedCardFiles = Directory.EnumerateFiles(_cardsCacheFolder, "CardCache*.json");
            foreach (var cachedCardFile in cachedCardFiles)
            {
                var cards = _configurationSerializer.Deserialize<MtgFullCard[]>(cachedCardFile, new MtgFullCard[0]);
                _logger.LogDebug($"Loaded {cards.Length} cards of set {cards.First().SetCode}");

                allCards.AddRange(cards);
            }

            stopwatch.Stop();
            _logger.LogDebug($"Loading cards from cache took {stopwatch.Elapsed}");

            _cardRepository.SetCardData(allCards);
            InitializeDone?.Invoke(this, EventArgs.Empty);
        }

        public void LoadMissingCardData(IMtgSetRepository setRepository, bool forceUpdate)
        {
            var knownCards = _cardRepository.CardData.GroupBy(c => c.SetCode).ToDictionary(c => c.Key);
            foreach (var set in setRepository.SetData)
            {
                var cardCount = knownCards.ContainsKey(set.SetCode) ? knownCards[set.SetCode].Count() : 0;
                if (!forceUpdate && set.NumberOfCards <= cardCount)
                {
                    _logger?.LogDebug($"Skipping update of cards for set {set.SetCode} ({set.SetName})");
                    continue;
                }

                var cards = _scryfallService.LoadCardsOfSet(set.SetCode);
                _logger?.LogDebug($"Replacing cards of set {set.SetCode} ({set.SetName})");
                _cardRepository.ReplaceCardsForSet(cards, set.SetCode);
                SaveSetCards(cards, set.SetCode);

                set.NumberOfCards = cards.Length;
            }
        }

        public void UpdateCardDetails(MtgFullCard card, bool saveSetCards)
        {
            if (card?.UniqueId == null || !card.IsOutdated())
            {
                return;
            }

            var updated = _scryfallService.LoadCardByScryfallId(card.UniqueId);
            if (updated == null)
            {
                return;
            }

            // Now update the card
            updated.Adapt(card);
            card.LastUpdate = DateTime.UtcNow;

            if (saveSetCards)
            {
                SaveSetCards(card.SetCode);
            }
        }

        public void UpdateCardsOfSet(string setCode)
        {
            var updated = _scryfallService.LoadCardsOfSet(setCode);
            if (updated == null)
            {
                return;
            }

            try
            {
                var existingSetCards = _cardRepository.CardData.Where(c => c.SetCode == setCode).ToArray();
                if (!existingSetCards.Any())
                {
                    _cardRepository.ReplaceCardsForSet(updated, setCode);
                    return;
                }

                foreach (var updatedCard in updated)
                {
                    var existing = existingSetCards.FirstOrDefault(l => l.UniqueId == updatedCard.UniqueId);
                    if (existing == null)
                    {
                        continue;
                    }

                    updatedCard.LastUpdate = DateTime.UtcNow;
                    updatedCard.Adapt(existing);
                }
            }
            finally
            {
                SaveSetCards(setCode);
            }
        }

        public void LoadAllCardData()
        {
            // TODO: L�sche alte Dateien
            var data = _scryfallService
                .LoadAllCards()
                .GroupBy(c => c.SetCode);

            foreach (var group in data)
            {
                _cardRepository.ReplaceCardsForSet(group, group.Key);
                SaveSetCards(group.ToArray(), group.Key);
            }
        }

        public void SaveSetCards(string setCode)
        {
            var setCards = _cardRepository.CardData
                .Where(c => c.SetCode.Equals(setCode, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            SaveSetCards(setCards, setCode);
        }

        private void SaveSetCards(MtgFullCard[] cards, string setCode)
        {
            var file = Path.Combine(_cardsCacheFolder, $"CardCache{setCode}.json");
            _configurationSerializer.Serialize(file, cards);
            _logger.LogDebug($"Saved {cards.Length} cards of set {cards.First().SetCode} as {file}");
        }
    }
}