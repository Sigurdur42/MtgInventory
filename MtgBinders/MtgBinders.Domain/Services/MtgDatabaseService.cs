using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Linq;

namespace MtgBinders.Domain.Services
{
    internal class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly IMtgCardService _cardService;
        private readonly IMtgSetService _setService;
        private readonly IMtgSetRepository _setRepository;
        private readonly IMtgCardRepository _cardRepository;
        private readonly IMtgWantsListService _wantsService;
        private readonly ILogger _logger;
        // TODO: Domain events

        public MtgDatabaseService(
            ILoggerFactory loggerFactory,
            IMtgCardService cardService,
            IMtgSetService setService,
            IMtgSetRepository setRepository,
            IMtgCardRepository cardRepository,
            IMtgWantsListService wantsService)
        {
            _logger = loggerFactory.CreateLogger(nameof(MtgDatabaseService));
            _cardService = cardService;
            _setService = setService;
            _setRepository = setRepository;
            _cardRepository = cardRepository;
            _wantsService = wantsService;
        }

        public event EventHandler DatabaseUpdated;

        public MtgFullCard[] CardData => _cardRepository.CardData;
        public MtgSetInfo[] SetData => _setRepository.SetData;
        public DateTime? LastUpdated => _setService.LastUpdatedCacheAt;

        public int NumberOfCards => _cardRepository.NumberOfCards;
        public int NumberOfSets => _setRepository.NumberOfSets;

        public bool IsCardsMissing { get; private set; }

        public void Initialize()
        {
            _setService.Initialize();
            _cardService.Initialize();
            _wantsService.Initialize();

            AnalyseMissingCards();

            DatabaseUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateDatabase(bool forceUpdate)
        {
            _setService.UpdateSetsFromScryfall(!forceUpdate);

            _cardService.LoadMissingCardData(_setRepository, forceUpdate);

            _setService.WriteSetsToCache();

            AnalyseMissingCards();

            DatabaseUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateCardDetails(MtgFullCard card)
        {
            _cardService.UpdateCardDetails(card);
        }

        private void AnalyseMissingCards()
        {
            IsCardsMissing = false;
            var cardCountBySet = CardData.GroupBy(c => c.SetCode).ToDictionary(c => c.Key);
            foreach (var set in SetData)
            {
                if (cardCountBySet.ContainsKey(set.SetCode))
                {
                    var cards = cardCountBySet[set.SetCode].Count();
                    if (cards < set.NumberOfCards)
                    {
                        _logger.LogInformation($"Set {set.SetCode} ({set.SetName}) is missing cards ({cards} loaded of {set.NumberOfCards})");
                        IsCardsMissing = true;
                    }
                }
            }
        }
    }
}