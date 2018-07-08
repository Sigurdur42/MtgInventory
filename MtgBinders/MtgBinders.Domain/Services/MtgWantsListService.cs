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
    internal class MtgWantsListService : IMtgWantsListService
    {
        private readonly string _configurationFileName;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly ILogger _logger;
        private readonly IMtgCardRepository _cardRepository;

        private List<MtgWantListCard> _wants = new List<MtgWantListCard>();

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

        public IEnumerable<MtgWantListCard> Wants => _wants;

        public void Initialize()
        {
            _logger.LogDebug("Starting initialize...");

            var existingWants = _configurationSerializer.Deserialize<IEnumerable<MtgWantListCard>>(_configurationFileName);
            if (existingWants != null)
            {
                _wants.AddRange(existingWants.Select(UpdateFullCard));
            }
        }

        public MtgWantListCard AddWant(MtgFullCard card, int count)
        {
            var result = InternalAdd(card, count);

            SaveWants();
            _logger.LogDebug($"Added want by {count} for {card.Name} ({card.SetCode})");
            return result;
        }

        private MtgWantListCard UpdateFullCard(MtgWantListCard card)
        {
            if (_cardRepository.CardsByUniqueId.TryGetValue(card.CardId, out var fullCard))
            {
                card.FullCard = fullCard;
            }

            return card;
        }

        private MtgWantListCard InternalAdd(MtgFullCard card, int count)
        {
            var existing = _wants.FirstOrDefault(c => c.CardId.Equals(card.UniqueId));
            if (existing == null)
            {
                existing = new MtgWantListCard
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

            return existing;
        }

        private void SaveWants()
        {
            _configurationSerializer.Serialize<IEnumerable<MtgWantListCard>>(_configurationFileName, _wants);
        }
    }
}