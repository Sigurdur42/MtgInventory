using MtgBinders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.Services
{
    internal class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly IMtgCardService _cardService;
        private readonly IMtgSetService _setService;
        private readonly IMtgSetRepository _setRepository;

        // TODO: Domain events

        public MtgDatabaseService(
            IMtgCardService cardService,
            IMtgSetService setService,
            IMtgSetRepository setRepository)
        {
            _cardService = cardService;
            _setService = setService;
            _setRepository = setRepository;
        }

        public void Initialize()
        {
            _setService.Initialize();
            _cardService.Initialize();
        }

        public void UpdateDatabase(bool forceUpdate)
        {
            _setService.UpdateSetsFromScryfall(!forceUpdate);
            _cardService.LoadMissingCardData(_setRepository);
        }
    }
}