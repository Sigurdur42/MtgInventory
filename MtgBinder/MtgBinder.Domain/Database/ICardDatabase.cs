using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using MtgDomain;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Database
{
    public interface ICardDatabase
    {
        event EventHandler DatabaseInitialized;

        event EventHandler CardsLoaded;

        event EventHandler SetsLoaded;

        event EventHandler InventoryChanged;

        int SetCount { get; }
        int CardCount { get; }
        ILiteCollection<SetInfo> Sets { get; }

        ILiteCollection<CardInfo> Cards { get; }

        void UpdateSetDataFromSryfall();

        void UpdateMissingCardDataFromSryfall();

        IEnumerable<CardInfo> LookupCards(string cardName, SearchOptions.RollupMode rollupMode);

        void Initialize(DirectoryInfo configurationFolder);

        void LoadCardsForSet(string setCode);

        CardPrice LookupLatestPrice(Guid scryfallId);

        void Close();
    }
}