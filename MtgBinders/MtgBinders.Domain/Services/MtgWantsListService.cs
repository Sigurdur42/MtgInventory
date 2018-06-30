using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MtgBinders.Domain.Services
{
    // TODO: Wants view + Option to add card

    internal class MtgWantsListService : IMtgWantsListService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;
        private readonly IMtgCardRepository _cardRepository;

        private List<WantListCard> _wants = new List<WantListCard>();

        public MtgWantsListService(
            IJsonConfigurationSerializer configurationSerializer,
            IBinderDomainConfigurationProvider configurationProvider,
            ILoggerFactory loggerFactory,
            IMtgCardRepository cardRepository)

        {
            _logger = loggerFactory.CreateLogger(nameof(MtgWantsListService));
            _configurationSerializer = configurationSerializer;
            _configurationFileName = Path.Combine(configurationProvider.AppDataFolder, "Wants.json");
            _cardRepository = cardRepository;
            _logger.LogDebug($"Configuarion details: {Environment.NewLine}- Configuration: {_configurationFileName}");
        }

        public IEnumerable<WantListCard> Wants => _wants;

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");

            var existingWants = _configurationSerializer.Deserialize<IEnumerable<WantListCard>>(_configurationFileName);
            if (existingWants != null)
            {
                _wants.AddRange(existingWants.Select(UpdateFullCard));
            }
        }

        public void AddWant(MtgFullCard card, int count)
        {
            InternalAdd(card, count);

            SaveWants();
        }

        private WantListCard UpdateFullCard(WantListCard card)
        {
            if (_cardRepository.CardsByUniqueId.TryGetValue(card.CardId, out var fullCard))
            {
                card.FullCard = fullCard;
            }

            return card;
        }

        private void InternalAdd(MtgFullCard card, int count)
        {
            var existing = _wants.FirstOrDefault(c => c.CardId.Equals(card.UniqueId));
            if (existing == null)
            {
                existing = new WantListCard
                {
                    CardId = card.UniqueId,
                    WantCount = count,
                    FullCard = card
                };
                _wants.Add(existing);
            }
            else
            {
                existing.WantCount += count;
            }
        }

        private void SaveWants()
        {
            _configurationSerializer.Serialize<IEnumerable<WantListCard>>(_configurationFileName, _wants);
        }
    }
}