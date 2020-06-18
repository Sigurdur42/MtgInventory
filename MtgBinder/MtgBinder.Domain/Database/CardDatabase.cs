using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using MtgBinder.Domain.Inventory;
using MtgBinder.Domain.Scryfall;
using MtgBinder.Domain.Tools;
using MtgDomain;
using PropertyChanged;
using ScryfallApi.Client.Models;
using Serilog;

namespace MtgBinder.Domain.Database
{
    [AddINotifyPropertyChangedInterface]
    public class CardDatabase : ICardDatabase
    {
        private readonly IScryfallService _scryfallService;
        private readonly IAsyncProgressNotifier _progressNotifier;

        private LiteDatabase _cardDatabase;
        private LiteDatabase _priceDatabase;
        private LiteDatabase _inventoryDatabase;

        private ILiteCollection<SetInfo> _setCollection;
        private ILiteCollection<CardInfo> _cardCollection;
        private ILiteCollection<CardPrice> _priceCollection;
        private ILiteCollection<InventoryItem> _inventoryCollection;

        public CardDatabase(
            IScryfallService scryfallService,
            IAsyncProgressNotifier progressNotifier)
        {
            _scryfallService = scryfallService;
            _progressNotifier = progressNotifier;
            SetCount = 42;
        }

        public event EventHandler DatabaseInitialized;

        public event EventHandler CardsLoaded;

        public event EventHandler SetsLoaded;
        public event EventHandler InventoryChanged;

        public ILiteCollection<SetInfo> Sets => _setCollection;

        public ILiteCollection<CardInfo> Cards => _cardCollection;

        public ILiteCollection<CardPrice> Prices => _priceCollection;

        public ILiteCollection<InventoryItem> Inventory => _inventoryCollection;

        public int SetCount { get; private set; }
        public int CardCount { get; private set; }

        public void UpdateSetDataFromSryfall()
        {
            using var logger = new ActionLogger(nameof(CardDatabase), nameof(UpdateSetDataFromSryfall));

            var sets = _scryfallService.RetrieveSets();

            _cardDatabase.BeginTrans();
            _setCollection.DeleteAll();

            _setCollection.InsertBulk(sets);
            EnsureSetIndex();
            _cardDatabase.Commit();

            SetCount = _setCollection.Count();

            SetsLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateMissingCardDataFromSryfall()
        {
            using var logger = new ActionLogger(nameof(CardDatabase), nameof(UpdateMissingCardDataFromSryfall));

            foreach (var setInfo in _setCollection.FindAll().OrderByDescending(s => s.ReleaseDate))
            {
                var cardCount = _cardCollection.Count(c => c.SetCode == setInfo.Code);
                if (cardCount == setInfo.CardCount)
                {
                    continue;
                }

                LoadCardsForSet(setInfo.Code);
            }
        }

        public IEnumerable<CardInfo> LookupCards(string cardName, SearchOptions.RollupMode rollupMode)
        {
            using var logger = new ActionLogger(nameof(CardDatabase), nameof(LookupCards) + "(" + cardName + ")");

            try
            {
                _progressNotifier.Start(logger.Prefix, 2);
                var cards = _scryfallService.RetrieveCardsByCardName(cardName, rollupMode);
                _progressNotifier.NextStep(logger.Prefix);
                InsertOrUpdateCards(cards);
                _progressNotifier.NextStep(logger.Prefix);

                return cards.Select(c => c.Card).ToArray();
            }
            finally
            {
                _progressNotifier.Finish(logger.Prefix);
            }
        }

        public void LoadCardsForSet(string setCode)
        {
            using var logger = new ActionLogger(nameof(CardDatabase), nameof(LoadCardsForSet) + "(" + setCode + ")");
            try
            {
                _progressNotifier.Start(logger.Prefix, 2);
                var cards = _scryfallService.RetrieveCardsForSetCode(setCode);
                _progressNotifier.NextStep(logger.Prefix);

                InsertOrUpdateCards(cards);
                _progressNotifier.NextStep(logger.Prefix);

                CardsLoaded?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _progressNotifier.Finish(logger.Prefix);
            }
        }

        public void Initialize(DirectoryInfo configurationFolder)
        {
            using var logger = new ActionLogger(
                nameof(CardDatabase),
                nameof(Initialize));

            logger.Information("Initialize on folder " + configurationFolder.FullName);

            var databaseFile = Path.Combine(configurationFolder.FullName, "LocalCache.db");
            _cardDatabase = new LiteDatabase(databaseFile);

            // Initialize set data
            _setCollection = _cardDatabase.GetCollection<SetInfo>();
            EnsureSetIndex();
            SetCount = _setCollection.Count();
            logger.Information($"Loaded set collection with {SetCount} records");

            _cardCollection = _cardDatabase.GetCollection<CardInfo>();
            EnsureCardIndex();
            CardCount = _cardCollection.Count();

            logger.Information($"Loaded card collection with {CardCount} records");

            databaseFile = Path.Combine(configurationFolder.FullName, "PriceData.db");
            _priceDatabase = new LiteDatabase(databaseFile);
            _priceCollection = _priceDatabase.GetCollection<CardPrice>();
            EnsurePriceIndex();
            logger.Information($"Loaded price data with {_priceCollection.Count()} records");

            databaseFile = Path.Combine(configurationFolder.FullName, "Inventory.db");
            _inventoryDatabase = new LiteDatabase(databaseFile);
            _inventoryCollection = _inventoryDatabase.GetCollection<InventoryItem>();
            EnsureInventoryIndex();
            logger.Information($"Loaded inventory data with {_inventoryCollection.Count()} records");

            DatabaseInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            Log.Information("Closing database");

            _cardDatabase?.Dispose();
            _cardDatabase = null;

            _priceDatabase?.Dispose();
            _priceDatabase = null;

            _inventoryDatabase?.Dispose();
            _inventoryDatabase = null;
        }

        public CardPrice LookupLatestPrice(Guid scryfallId)
        {
            using var logger = new ActionLogger(
                nameof(CardDatabase),
                nameof(LookupLatestPrice) + $"({scryfallId})");

            // TODO: Optimize this query
            var data = _priceCollection.Find(p => p.ScryfallId == scryfallId).ToArray();

            return data.OrderByDescending(p => p.DateUtc).FirstOrDefault() ?? new CardPrice();
        }

        private void EnsureInventoryIndex()
        {
            _inventoryCollection.EnsureIndex(i => i.CardId);
            _inventoryCollection.EnsureIndex(i => i.CardName);
        }

        private void EnsurePriceIndex()
        {
            _priceCollection.EnsureIndex(c => c.Id);
            _priceCollection.EnsureIndex(c => c.ScryfallId);
            _priceCollection.EnsureIndex(c => c.DateTimeLookup);
        }

        private void InsertOrUpdateCards(ScryfallCardData[] cards)
        {
            using var logger = new ActionLogger(
                nameof(CardDatabase),
                nameof(InsertOrUpdateCards) + $"({cards.Length})");

            var inserted = 0;
            foreach (var cardInfo in cards)
            {
                var found = _cardCollection.FindOne(c => c.Id == cardInfo.Card.Id);
                if (found != null)
                {
                    _cardCollection.Update(found.Id, cardInfo.Card);
                }
                else
                {
                    _cardCollection.Insert(cardInfo.Card);
                    inserted += 1;
                }

                var foundPrice = _priceCollection.FindOne(p => p.Id == cardInfo.Price.ScryfallId && p.DateTimeLookup == cardInfo.Price.DateTimeLookup);
                if (foundPrice == null)
                {
                    _priceCollection.Insert(cardInfo.Price);
                }
            }

            if (inserted > 0)
            {
                EnsureCardIndex();
                EnsurePriceIndex();
            }

            logger.Result = $"Update: {cards.Length - inserted}, Inserted: {inserted}";
        }

        private void EnsureCardIndex()
        {
            _cardCollection.EnsureIndex(c => c.Id);
            _cardCollection.EnsureIndex(c => c.Name);
            _cardCollection.EnsureIndex(c => c.SetCode);
        }

        private void EnsureSetIndex()
        {
            _setCollection.EnsureIndex(s => s.Code);
            _setCollection.EnsureIndex(s => s.Name);
        }
    }
}