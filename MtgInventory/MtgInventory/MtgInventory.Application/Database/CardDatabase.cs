using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LiteDB;
using MkmApi;
using MkmApi.Entities;
using MtgInventory.Service.Converter;
using MtgInventory.Service.Models;
using Serilog;

namespace MtgInventory.Service.Database
{
    public sealed class CardDatabase : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<CardDatabase>();

        private LiteDatabase _cardDatabase;
        private LiteDatabase _scryfallDatabase;
        private LiteDatabase _mkmDatabase;
        private LiteDatabase _priceDatabase;

        public bool IsInitialized { get; private set; }

        public ILiteCollection<MkmProductInfo> MkmProductInfo { get; private set; }

        public ILiteCollection<Expansion> MkmExpansion { get; private set; }
        public ILiteCollection<ApiCallStatistics> ApiCallStatistics { get; private set; }
        public ILiteCollection<MkmAdditionalCardInfo> MkmAdditionalInfo { get; private set; }

        public ILiteCollection<ScryfallCard> ScryfallCards { get; private set; }
        public ILiteCollection<ScryfallSet> ScryfallSets { get; private set; }

        public ILiteCollection<DetailedSetInfo> MagicSets { get; private set; }
        public ILiteCollection<DetailedMagicCard> MagicCards { get; private set; }
        public ILiteCollection<CardPrice> CardPrices { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize(
            DirectoryInfo folder)
        {
            _logger.Information($"{nameof(Initialize)}: Initializing database service...");

            folder.EnsureExists();
            var databaseFile = Path.Combine(folder.FullName, "CardDatabase.db");
            _cardDatabase = new LiteDatabase(databaseFile);
            _scryfallDatabase = new LiteDatabase(Path.Combine(folder.FullName, "ScryfallDatabase.db"));
            _mkmDatabase = new LiteDatabase(Path.Combine(folder.FullName, "MkmDatabase.db"));
            _priceDatabase = new LiteDatabase(Path.Combine(folder.FullName, "PriceDatabase.db"));

            MkmProductInfo = _mkmDatabase.GetCollection<MkmProductInfo>();
            MkmExpansion = _mkmDatabase.GetCollection<Expansion>();
            ApiCallStatistics = _mkmDatabase.GetCollection<ApiCallStatistics>();
            MkmAdditionalInfo = _mkmDatabase.GetCollection<MkmAdditionalCardInfo>();
            CardPrices = _priceDatabase.GetCollection<CardPrice>();

            ScryfallCards = _scryfallDatabase.GetCollection<ScryfallCard>();
            ScryfallSets = _scryfallDatabase.GetCollection<ScryfallSet>();

            MagicSets = _cardDatabase.GetCollection<DetailedSetInfo>();
            MagicCards = _cardDatabase.GetCollection<DetailedMagicCard>();

            ////var mapper = BsonMapper.Global;

            ////mapper.Entity<DetailedMagicCard>()
            ////    .Id(x => x.Id);

            IsInitialized = true;
        }

        public void ShutDown()
        {
            _logger.Information($"{nameof(ShutDown)}: Shutting down database service...");

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

        public void InsertScryfallSets(IEnumerable<ScryfallSet> sets)
        {
            _logger.Information($"{nameof(InsertScryfallSets)}: Cleaning existing set info...");
            ScryfallSets.DeleteAll();
            ScryfallSets.InsertBulk(sets);

            ScryfallSets.EnsureIndex(e => e.Code);
            ScryfallSets.EnsureIndex(e => e.Name);
        }

        public void InsertScryfallCards(IEnumerable<ScryfallCard> cards)
        {
            _logger.Information($"{nameof(InsertScryfallCards)}: Inserting new scryfall cards...");
            ScryfallCards.InsertBulk(cards);

            ScryfallCards.EnsureIndex(e => e.Set);
            ScryfallCards.EnsureIndex(e => e.Name);
        }

        public void ClearScryfallCards()
        {
            _logger.Information($"{nameof(ClearScryfallCards)}: Cleaning existing card info...");
            ScryfallCards.DeleteAll();
        }

        public void ClearDetailedCards()
        {
            _logger.Information($"{nameof(ClearDetailedCards)}: Cleaning existing detailed card info...");
            MagicCards.DeleteAll();
        }

        public void EnsureCardPriceIndex()
        {
            CardPrices.EnsureIndex(c => c.MkmId);
            CardPrices.EnsureIndex(c => c.ScryfallId);
            CardPrices.EnsureIndex(c => c.Source);
            CardPrices.EnsureIndex(c => c.UpdateDate);
        }

        public void UpdateMkmAdditionalInfo(
            string mkmId,
            string mkmWebSite)
        {
            var found = FindAdditionalMkmInfo(mkmId);
            if (found == null)
            {
                found = new MkmAdditionalCardInfo
                {
                    MkmId = mkmId,
                };

                MkmAdditionalInfo.Insert(found);
            }

            found.MkmWebSite = mkmWebSite;
            MkmAdditionalInfo.Update(found);
            MkmAdditionalInfo.EnsureIndex(c => c.MkmId);
        }

        public MkmAdditionalCardInfo FindAdditionalMkmInfo(string mkmId)
            => MkmAdditionalInfo.Query().Where(c => c.MkmId == mkmId).FirstOrDefault();

        public void InsertProductInfo(
            IEnumerable<ProductInfo> products,
            IEnumerable<Expansion> expansions)
        {
            _logger.Information($"{nameof(InsertProductInfo)}: Cleaning existing product info...");
            MkmProductInfo.DeleteAll();

            var orderedExpansions = expansions.ToDictionary(_ => _.IdExpansion.ToString(CultureInfo.InvariantCulture) ?? "");

            var temp = new List<MkmProductInfo>();
            var total = 0;

            foreach (var p in products)
            {
                var product = new MkmProductInfo(p);
                if (orderedExpansions.TryGetValue(p.ExpansionId?.ToString(CultureInfo.InvariantCulture) ?? "", out var foundExpansion))
                {
                    product.ExpansionName = foundExpansion.EnName;
                    product.ExpansionCode = foundExpansion.Abbreviation;
                }

                temp.Add(product);

                if (temp.Count >= 1000)
                {
                    total += temp.Count;
                    _logger.Information($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total})...");

                    BulkInsertProductInfo(temp);

                    temp.Clear();
                }
            }

            total += temp.Count;
            _logger.Information($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total})...");
            BulkInsertProductInfo(temp);
        }

        public void UpdateMkmStatistics(IApiCallStatistic mkmStatistic)
        {
            var found = ApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                var newRecord = new ApiCallStatistics
                {
                    CountToday = mkmStatistic.CountToday,
                    CountTotal = mkmStatistic.CountTotal,
                    Today = mkmStatistic.Today,
                };
                ApiCallStatistics.Insert(newRecord);
            }
            else
            {
                found.CountToday = mkmStatistic.CountToday;
                found.CountTotal = mkmStatistic.CountTotal;
                found.Today = mkmStatistic.Today;
                ApiCallStatistics.Update(found);
            }
        }

        public IApiCallStatistic GetMkmCallStatistic()
        {
            var found = ApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                found = new ApiCallStatistics
                {
                    CountToday = 0,
                    CountTotal = 0,
                    Today = DateTime.Now.Date,
                };
                ApiCallStatistics.Insert(found);
            }

            return found;
        }

        public void RebuildDetailedDatabase()
        {
            var setData = RebuildSetData();
            RebuildDetailedCardData(setData);
        }

        internal void RebuildDetailedCardData(Dictionary<string, DetailedSetInfo> indexedSetData)
        {
            ClearDetailedCards();

            var indexedCards = new Dictionary<string, DetailedMagicCard>();

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding MKM card data...");
            DetailedSetInfo lastSet = null;

            foreach (var mkm in MkmProductInfo.Query().Where(c => c.CategoryId == 1).ToArray().OrderBy(c => c.ExpansionCode))
            {
                if (lastSet?.SetCodeMkm != mkm.ExpansionCode)
                {
                    indexedSetData.TryGetValue(mkm.ExpansionCode, out lastSet);
                }

                var key = $"{mkm.ExpansionCode}_{mkm.Name}".ToUpperInvariant();
                if (indexedCards.TryGetValue(key, out var found))
                {
                    // Add it anyway but extend the key
                    key += "_" + mkm.Id;
                }

                var card = new DetailedMagicCard();
                card.UpdateFromMkm(mkm, lastSet);
                indexedCards.Add(key, card);
            }

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - Inserting cards now...");
            MagicCards.InsertBulk(indexedCards.Values);

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - Updating index...");
            EnsureMagicCardsIndex();

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding Scryfall card data...");
            indexedCards.Clear();

            var cardsToInsert = new List<DetailedMagicCard>();
            var nonUniqueCards = new List<ScryfallCard>();

            var allScryfallCards = ScryfallCards.FindAll().ToArray();
            var remaining = allScryfallCards.Length;

            foreach (var scryfallCard in allScryfallCards)
            {
                var foundMkmCards = MagicCards.Query().Where(c => c.NameEn == scryfallCard.Name && c.SetCode == scryfallCard.Set).ToList();
                var found = foundMkmCards.FirstOrDefault();

                if (!foundMkmCards.Any())
                {
                    found = new DetailedMagicCard();
                    cardsToInsert.Add(found);
                }
                else if (foundMkmCards.Count > 1)
                {
                    // Select the correct one
                    nonUniqueCards.Add(scryfallCard);

                    continue;
                }

                var setCode = scryfallCard.Set?.ToUpperInvariant();

                if (lastSet?.SetCodeScryfall != scryfallCard.Set)
                {
                    indexedSetData.TryGetValue(setCode, out lastSet);
                }

                found.UpdateFromScryfall(scryfallCard, lastSet);

                --remaining;
                if (remaining % 1000 == 0)
                {
                    Log.Debug($"Updating Scryfall cards - {remaining}");

                    MagicCards.InsertBulk(cardsToInsert);
                    cardsToInsert.Clear();

                    EnsureMagicCardsIndex();
                }
            }

            // insert the remaining cards
            MagicCards.InsertBulk(cardsToInsert);
            cardsToInsert.Clear();

            EnsureMagicCardsIndex();

            // Now handle differnet arts of the same card
            if (nonUniqueCards.Any())
            {
                var grouped = nonUniqueCards.GroupBy(c => $"{c.Name}_{c.Set}");
                remaining = grouped.Count();

                foreach (var group in grouped)
                {
                    --remaining;
                    var first = group.First();

                    // get all these cards from actual Scryfall local DB first
                    var scryfallCards = ScryfallCards.Query().Where(c => c.Set == first.Set && c.Name == first.Name).OrderBy(c => c.CollectorNumber).ToArray();

                    if (lastSet?.SetCodeScryfall != first.Set)
                    {
                        indexedSetData.TryGetValue(first.Set, out lastSet);
                    }

                    var existingCards = MagicCards.Query()
                        .Where(c => c.NameEn == first.Name && c.SetCode == first.Set)
                        .OrderBy(c => c.MkmMetaCardId)
                        .ToArray();

                    if (existingCards.Length != scryfallCards.Length)
                    {
                        Log.Debug($"{nameof(RebuildDetailedDatabase)} - {group.Key} different card count. MKM={existingCards.Length} Scryfall: {scryfallCards.Length}");
                    }

                    // TODO: Handle Oversized cards

                    for (var index = 0; index < Math.Min(existingCards.Length, scryfallCards.Length); ++index)
                    {
                        existingCards[index].UpdateFromScryfall(scryfallCards[index], lastSet);
                    }

                    MagicCards.Update(existingCards);
                }

                nonUniqueCards.Clear();

                EnsureMagicCardsIndex();
            }

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - Done rebuilding Scryfall card data...");
        }

        internal Dictionary<string, DetailedSetInfo> RebuildSetData()
        {
            // Set data
            MagicSets.DeleteAll();
            var indexedSets = new Dictionary<string, DetailedSetInfo>();

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding MKM Set data...");
            foreach (var mkm in MkmExpansion.FindAll())
            {
                var key = mkm.Abbreviation.ToUpperInvariant();
                if (indexedSets.TryGetValue(key, out var found))
                {
                    Log.Warning($"Duplicate MKM set found: {mkm}");
                    continue;
                }

                var set = new DetailedSetInfo();
                set.UpdateFromMkm(mkm);
                indexedSets.Add(key, set);
            }

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding Scryfall Set data...");
            foreach (var scryfall in ScryfallSets.FindAll())
            {
                if (scryfall.Code == null)
                {
                    continue;
                }

                var key = scryfall.Code?.ToUpperInvariant();
                if (!indexedSets.TryGetValue(key, out var found))
                {
                    found = new DetailedSetInfo();
                    indexedSets.Add(key, found);
                }

                found.UpdateFromScryfall(scryfall);
            }

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - inserting sets now...");
            MagicSets.InsertBulk(indexedSets.Values);

            Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding set index...");
            MagicSets.EnsureIndex(s => s.SetNameScryfall);
            MagicSets.EnsureIndex(s => s.SetNameMkm);
            MagicSets.EnsureIndex(s => s.SetCodeScryfall);
            MagicSets.EnsureIndex(s => s.SetCodeMkm);

            return indexedSets;
        }

        internal void InsertExpansions(IEnumerable<Expansion> expansions)
        {
            foreach (var exp in expansions)
            {
                exp.ReleaseDateParsed = exp.ReleaseDate.ParseDateTime();
            }

            MkmExpansion.DeleteAll();
            MkmExpansion.InsertBulk(expansions);

            MkmExpansion.EnsureIndex(e => e.EnName);
            MkmExpansion.EnsureIndex(e => e.IdExpansion);
            MkmExpansion.EnsureIndex(e => e.IdGame);
        }

        private void EnsureMagicCardsIndex()
        {
            MagicCards.EnsureIndex(c => c.MkmId);
            MagicCards.EnsureIndex(c => c.ScryfallId);
            MagicCards.EnsureIndex(c => c.NameEn);
            MagicCards.EnsureIndex(c => c.SetCode);
            MagicCards.EnsureIndex(c => c.IsBasicLand);
            MagicCards.EnsureIndex(c => c.IsToken);
        }

        private void BulkInsertProductInfo(IList<MkmProductInfo> products)
        {
            MkmProductInfo.InsertBulk(products);
            MkmProductInfo.EnsureIndex(p => p.Name);
            MkmProductInfo.EnsureIndex(p => p.CategoryId);
        }
    }
}