using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;
using System;

namespace MtgBinders.Domain.Services
{
    public interface IMtgCardService
    {
        event EventHandler InitializeDone;

        int NumberOfCards { get; }

        void Initialize();

        void LoadMissingCardData(IMtgSetRepository setRepository, bool forceUpdate);

        void LoadAllCardData();

        void SaveSetCards(string setCode);

        void UpdateCardDetails(MtgFullCard card, bool saveSetCards);

        void UpdateCardsOfSet(string setCode);
    }
}