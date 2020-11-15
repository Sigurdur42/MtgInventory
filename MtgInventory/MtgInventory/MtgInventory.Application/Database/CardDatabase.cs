using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using MkmApi;
using MkmApi.Entities;
using MtgInventory.Service.Converter;
using MtgInventory.Service.Models;
using ScryfallApiServices;

namespace MtgInventory.Service.Database
{
    public sealed class CardDatabase : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CardDatabase> _logger;
        private LiteDatabase? _cardDatabase;
        private DetailedDatabaseBuilder? _detailedDatabaseBuilder;
        private LiteDatabase? _mkmDatabase;
        private LiteDatabase? _priceDatabase;
        private LiteDatabase? _scryfallDatabase;
        public ILiteCollection<CardPrice>? CardPrices { get; private set; }
        public bool IsInitialized { get; private set; }

        public ILiteCollection<DetailedMagicCard>? MagicCards { get; private set; }
        public ILiteCollection<DetailedSetInfo>? MagicSets { get; private set; }
        public ILiteCollection<MkmAdditionalCardInfo>? MkmAdditionalInfo { get; private set; }
        public ILiteCollection<ApiCallStatistics>? MkmApiCallStatistics { get; private set; }
        public ILiteCollection<Expansion>? MkmExpansion { get; private set; }
        public ILiteCollection<MkmProductInfo>? MkmProductInfo { get; private set; }
        public ILiteCollection<ScryfallApiCallStatistic>? ScryfallApiCallStatistics { get; private set; }
        public ILiteCollection<ScryfallCard>? ScryfallCards { get; private set; }
        public ILiteCollection<ScryfallSet>? ScryfallSets { get; private set; }

        public CardDatabase(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<CardDatabase>();
        }

        public void ClearDetailedCards()
        {
            _logger.LogInformation($"{nameof(ClearDetailedCards)}: Cleaning existing detailed card info...");
            MagicCards?.DeleteAll();
        }

        public void ClearScryfallCards()
        {
            _logger.LogInformation($"{nameof(ClearScryfallCards)}: Cleaning existing card info...");
            ScryfallCards?.DeleteAll();
        }

        public void Dispose()
        {
            ShutDown();
        }

        public void EnsureCardPriceIndex()
        {
            CardPrices?.EnsureIndex(c => c.MkmId);
            CardPrices?.EnsureIndex(c => c.ScryfallId);
            CardPrices?.EnsureIndex(c => c.Source);
            CardPrices?.EnsureIndex(c => c.UpdateDate);
        }

        public MkmAdditionalCardInfo? FindAdditionalMkmInfo(string mkmId)
            => MkmAdditionalInfo.Query().Where(c => c.MkmId == mkmId).FirstOrDefault();

        public IApiCallStatistic GetMkmCallStatistic()
        {
            var found = MkmApiCallStatistics?.Query().FirstOrDefault();
            if (found == null)
            {
                found = new ApiCallStatistics
                {
                    CountToday = 0,
                    CountTotal = 0,
                    Today = DateTime.Now.Date,
                };
                MkmApiCallStatistics?.Insert(found);
            }

            return found;
        }

        public IScryfallApiCallStatistic GetScryfallApiStatistics()
        {
            var found = ScryfallApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                found = new ScryfallApiCallStatistic
                {
                    ScryfallCountToday = 0,
                    ScryfallCountTotal = 0,
                    Today = DateTime.Now.Date,
                };
                ScryfallApiCallStatistics.Insert(found);
            }

            return found;
        }

        public void Initialize(DirectoryInfo folder)
        {
            // _// Logger.Information($"{nameof(Initialize)}: Initializing database service...");
            _detailedDatabaseBuilder = new DetailedDatabaseBuilder(this, _loggerFactory);

            folder.EnsureExists();
            var databaseFile = Path.Combine(folder.FullName, "CardDatabase.db");
            _cardDatabase = new LiteDatabase(databaseFile);

            _scryfallDatabase = new LiteDatabase(Path.Combine(folder.FullName, "ScryfallDatabase.db"));
            _mkmDatabase = new LiteDatabase(Path.Combine(folder.FullName, "MkmDatabase.db"));
            _priceDatabase = new LiteDatabase(Path.Combine(folder.FullName, "PriceDatabase.db"));

            MkmProductInfo = _mkmDatabase.GetCollection<MkmProductInfo>();
            MkmExpansion = _mkmDatabase.GetCollection<Expansion>();
            MkmApiCallStatistics = _mkmDatabase.GetCollection<ApiCallStatistics>();
            ScryfallApiCallStatistics = _mkmDatabase.GetCollection<ScryfallApiCallStatistic>();
            MkmAdditionalInfo = _mkmDatabase.GetCollection<MkmAdditionalCardInfo>();
            CardPrices = _priceDatabase.GetCollection<CardPrice>();

            ScryfallCards = _scryfallDatabase.GetCollection<ScryfallCard>();
            ScryfallSets = _scryfallDatabase.GetCollection<ScryfallSet>();

            MagicSets = _cardDatabase.GetCollection<DetailedSetInfo>();
            MagicCards = _cardDatabase.GetCollection<DetailedMagicCard>();

            var mapper = BsonMapper.Global;

            mapper.Entity<DetailedMagicCard>()
                .Id(x => x.Id)
                .Ignore(x => x.IsScryfallOnly)
                .Ignore(x => x.MkmDetailsRequired);

            IsInitialized = true;
        }

        public void InsertProductInfo(
            IEnumerable<ProductInfo> products,
            IEnumerable<Expansion> expansions)
        {
            _logger.LogInformation($"{nameof(InsertProductInfo)}: Cleaning existing product info...");
            MkmProductInfo?.DeleteAll();

            var orderedExpansions = expansions.ToDictionary(_ => _.IdExpansion.ToString(CultureInfo.InvariantCulture) ?? "");

            var temp = new List<MkmProductInfo>();
            var total = 0;

            const int pageSize = 5000;

            foreach (var p in products)
            {
                var product = new MkmProductInfo(p);
                if (orderedExpansions.TryGetValue(p.ExpansionId?.ToString(CultureInfo.InvariantCulture) ?? "", out var foundExpansion))
                {
                    product.ExpansionName = foundExpansion.EnName;
                    product.ExpansionCode = foundExpansion.Abbreviation;
                }

                temp.Add(product);

                if (temp.Count >= pageSize)
                {
                    total += temp.Count;
                    _logger.LogInformation($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total})...");

                    BulkInsertProductInfo(temp);

                    temp.Clear();
                }
            }

            total += temp.Count;
            _logger.LogInformation($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total})...");
            BulkInsertProductInfo(temp);
        }

        public void InsertScryfallCards(IEnumerable<ScryfallCard> cards)
        {
            // _// Logger.Information($"Inserting {cards.Count()} new scryfall cards...");
            ScryfallCards?.InsertBulk(cards);

            ScryfallCards?.EnsureIndex(e => e.Set);
            ScryfallCards?.EnsureIndex(e => e.Name);
        }

        public void InsertScryfallSets(IEnumerable<ScryfallSet> sets)
        {
            // _// Logger.Information($"{nameof(InsertScryfallSets)}: Cleaning existing set info...");
            ScryfallSets?.DeleteAll();
            ScryfallSets?.InsertBulk(sets);

            ScryfallSets?.EnsureIndex(e => e.Code);
            ScryfallSets?.EnsureIndex(e => e.Name);
        }

        public void RebuildDetailedDatabase()
        {
            RebuildSetData();
            RebuildDetailedCardData();
        }

        public void ShutDown()
        {
            _logger.LogInformation($"{nameof(ShutDown)}: Shutting down database service...");

            IsInitialized = false;
            _cardDatabase?.Dispose();
            _cardDatabase = null;
            _scryfallDatabase?.Dispose();
            _scryfallDatabase = null;
            _mkmDatabase?.Dispose();
            _mkmDatabase = null;
            _priceDatabase?.Dispose();
            _priceDatabase = null;
        }

        public MkmAdditionalCardInfo UpdateMkmAdditionalInfo(
            Product product)
        {
            var found = FindAdditionalMkmInfo(product.IdProduct);
            if (found == null)
            {
                found = new MkmAdditionalCardInfo
                {
                    MkmId = product.IdProduct,
                };

                MkmAdditionalInfo?.Insert(found);
            }

            var detailedCard = MagicCards?.Query().Where(c => c.MkmId == product.IdProduct).FirstOrDefault();
            if (detailedCard != null)
            {
                detailedCard.UpdateFromMkm(product);
                MagicCards?.Update(detailedCard);
            }

            found.UpdateFromProduct(product);
            MkmAdditionalInfo?.Update(found);
            MkmAdditionalInfo?.EnsureIndex(c => c.MkmId);

            return found;
        }

        public void UpdateMkmStatistics(IApiCallStatistic mkmStatistic)
        {
            var found = MkmApiCallStatistics?.Query()?.FirstOrDefault();
            if (found == null)
            {
                var newRecord = new ApiCallStatistics
                {
                    CountToday = mkmStatistic.CountToday,
                    CountTotal = mkmStatistic.CountTotal,
                    Today = mkmStatistic.Today,
                };
                MkmApiCallStatistics?.Insert(newRecord);
            }
            else
            {
                found.CountToday = mkmStatistic.CountToday;
                found.CountTotal = mkmStatistic.CountTotal;
                found.Today = mkmStatistic.Today;
                MkmApiCallStatistics?.Update(found);
            }
        }

        public void UpdateScryfallStatistics(IScryfallApiCallStatistic statistic)
        {
            var found = ScryfallApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                var newRecord = new ScryfallApiCallStatistic
                {
                    ScryfallCountToday = statistic.ScryfallCountToday,
                    ScryfallCountTotal = statistic.ScryfallCountTotal,
                    Today = statistic.Today,
                };
                ScryfallApiCallStatistics.Insert(newRecord);
            }
            else
            {
                found.ScryfallCountToday = statistic.ScryfallCountToday;
                found.ScryfallCountTotal = statistic.ScryfallCountTotal;
                found.Today = statistic.Today;
                ScryfallApiCallStatistics.Update(found);
            }
        }

        internal void EnsureMagicCardsIndex()
        {
            MagicCards?.EnsureIndex(c => c.MkmId);
            MagicCards?.EnsureIndex(c => c.ScryfallId);
            MagicCards?.EnsureIndex(c => c.NameEn);
            MagicCards?.EnsureIndex(c => c.SetCode);
            MagicCards?.EnsureIndex(c => c.SetName);
            MagicCards?.EnsureIndex(c => c.IsBasicLand);
            MagicCards?.EnsureIndex(c => c.IsToken);
            MagicCards?.EnsureIndex(c => c.IsMappedByReferenceCard);
        }

        internal void EnsureSetIndex()
        {
            MagicSets?.EnsureIndex(s => s.SetNameScryfall);
            MagicSets?.EnsureIndex(s => s.SetNameMkm);
            MagicSets?.EnsureIndex(s => s.SetCodeScryfall);
            MagicSets?.EnsureIndex(s => s.SetCodeMkm);
        }

        internal void InsertExpansions(IEnumerable<Expansion> expansions)
        {
            foreach (var exp in expansions)
            {
                exp.ReleaseDateParsed = exp.ReleaseDate.ParseDateTime();
            }

            MkmExpansion?.DeleteAll();
            MkmExpansion?.InsertBulk(expansions);

            MkmExpansion?.EnsureIndex(e => e.EnName);
            MkmExpansion?.EnsureIndex(e => e.IdExpansion);
            MkmExpansion?.EnsureIndex(e => e.IdGame);
        }

        internal void RebuildCardsForSet(DetailedSetInfo set)
        {
            _detailedDatabaseBuilder?.RebuildMkmCardsForSet(set);
            _detailedDatabaseBuilder?.RebuildScryfallCardsForSet(set.SetCodeScryfall);
        }

        internal void RebuildDetailedCardData()
        {
            _cardDatabase?.Pragma("CHECKPOINT", 10000);
            _scryfallDatabase?.Pragma("CHECKPOINT", 10000);
            _mkmDatabase?.Pragma("CHECKPOINT", 10000);

            try
            {
                ClearDetailedCards();

                _logger.LogDebug($"{nameof(RebuildDetailedDatabase)} - rebuilding MKM card data...");

                foreach (var mkm in MkmExpansion?.FindAll().OrderBy(c => c.Abbreviation))
                {
                    _detailedDatabaseBuilder?.RebuildMkmCardsForSet(mkm.Abbreviation);
                }

                _logger.LogDebug($"{nameof(RebuildDetailedDatabase)} - rebuilding Scryfall card data...");
                foreach (var scryfall in ScryfallSets?.FindAll()?.OrderBy(c => c.Code)?.ToArray() ?? new ScryfallSet[0])
                {
                    _detailedDatabaseBuilder?.RebuildScryfallCardsForSet(scryfall.Code);
                }

                _logger.LogDebug($"{nameof(RebuildDetailedDatabase)} - Done rebuilding Scryfall card data...");
            }
            finally
            {
                _cardDatabase?.Pragma("CHECKPOINT", 1000);
                _scryfallDatabase?.Pragma("CHECKPOINT", 1000);
                _mkmDatabase?.Pragma("CHECKPOINT", 1000);
            }
        }

        internal void RebuildSetData()
        {
            _detailedDatabaseBuilder?.BuildMkmSetData();
            _detailedDatabaseBuilder?.BuildScryfallSetData();
            EnsureSetIndex();
        }

        internal void ResetCardAndSetUpdateDate()
        {
            var allSets = MagicSets?.FindAll().ToArray();
            foreach (var detailedSetInfo in allSets)
            {
                detailedSetInfo.CardsLastUpdated = DateTime.Now.AddDays(-1000);
                detailedSetInfo.CardsLastUpdated = DateTime.Now.AddDays(-1000);
            }

            MagicSets?.Update(allSets);
        }

        private void BulkInsertProductInfo(IList<MkmProductInfo> products)
        {
            MkmProductInfo?.InsertBulk(products);
            MkmProductInfo?.EnsureIndex(p => p.Name);
            MkmProductInfo?.EnsureIndex(p => p.CategoryId);
        }
    }
}