using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MtgBinders.Domain.Services
{
    internal class MtgInventoryService : IMtgInventoryService
    {
        private readonly ILogger _logger;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly string _inventoryFileName;
        private readonly IMtgCardRepository _cardRepository;

        public MtgInventoryService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory,
            IMtgCardRepository cardRepository)
        {
            _logger = loggerFactory.CreateLogger(nameof(MtgInventoryService));
            _configurationSerializer = configurationSerializer;
            _inventoryFileName = Path.Combine(configurationProvider.AppDataFolder, "Inventory.json");
            _cardRepository = cardRepository;
        }

        public event EventHandler Initialized;

        public bool IsInitialized { get; private set; }
        public IList<MtgInventoryCard> Cards { get; private set; }

        public void Initialize()
        {
            _logger.LogDebug("Loading inventory...");

            var stopwatch = Stopwatch.StartNew();
            var inventoryCards = _configurationSerializer
                .Deserialize<MtgInventoryCard[]>(_inventoryFileName, new MtgInventoryCard[0])
                .AsParallel()
                .Select(FindFullCard)
                .ToList();

            Cards = inventoryCards;

            stopwatch.Stop();
            _logger.LogDebug($"Loading inventory took {stopwatch.Elapsed}");
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        public void SaveInventory()
        {
            if (Cards == null)
            {
                Cards = new List<MtgInventoryCard>();
            }

            _configurationSerializer.Serialize(_inventoryFileName, Cards.ToArray());
        }

        internal MtgInventoryCard FindFullCard(MtgInventoryCard card)
        {
            if (_cardRepository.CardsByUniqueId.TryGetValue(card.CardId, out var found))
            {
                card.FullCard = found;
            }

            return card;
        }
    }
}