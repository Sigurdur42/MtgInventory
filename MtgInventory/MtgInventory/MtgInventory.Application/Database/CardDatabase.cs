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
using ScryfallApiServices;
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

        private DetailedDatabaseBuilder _detailedDatabaseBuilder;
        public bool IsInitialized { get; private set; }

        public ILiteCollection<MkmProductInfo> MkmProductInfo { get; private set; }

        public ILiteCollection<Expansion> MkmExpansion { get; private set; }
        public ILiteCollection<ApiCallStatistics> MkmApiCallStatistics { get; private set; }
        public ILiteCollection<ScryfallApiCallStatistic> ScryfallApiCallStatistics { get; private set; }
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
            _detailedDatabaseBuilder = new DetailedDatabaseBuilder(this);

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
            var found = MkmApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                var newRecord = new ApiCallStatistics
                {
                    CountToday = mkmStatistic.CountToday,
                    CountTotal = mkmStatistic.CountTotal,
                    Today = mkmStatistic.Today,
                };
                MkmApiCallStatistics.Insert(newRecord);
            }
            else
            {
                found.CountToday = mkmStatistic.CountToday;
                found.CountTotal = mkmStatistic.CountTotal;
                found.Today = mkmStatistic.Today;
                MkmApiCallStatistics.Update(found);
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

        public IApiCallStatistic GetMkmCallStatistic()
        {
            var found = MkmApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                found = new ApiCallStatistics
                {
                    CountToday = 0,
                    CountTotal = 0,
                    Today = DateTime.Now.Date,
                };
                MkmApiCallStatistics.Insert(found);
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

        public void RebuildDetailedDatabase()
        {
            RebuildSetData();
            RebuildDetailedCardData();
        }

        internal void RebuildDetailedCardData()
        {
            _cardDatabase.Pragma("CHECKPOINT", 10000);
            _scryfallDatabase.Pragma("CHECKPOINT", 10000);
            _mkmDatabase.Pragma("CHECKPOINT", 10000);

            try
            {
                // Define common set code
                var indexedSetData = MagicSets.FindAll().Where(s=>!string.IsNullOrWhiteSpace(s.SetCodeMkm)).ToDictionary(s => s.SetCodeMkm);

                ClearDetailedCards();

                Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding MKM card data...");

                foreach (var mkm in MkmExpansion.FindAll().ToArray().OrderBy(c => c.Abbreviation))
                {
                    _detailedDatabaseBuilder.RebuildMkmCardsForSet(mkm.Abbreviation);
                }

                Log.Debug($"{nameof(RebuildDetailedDatabase)} - rebuilding Scryfall card data...");
                DetailedSetInfo lastSet = null;
                var cardsToInsert = new List<DetailedMagicCard>();
                var nonUniqueCards = new List<ScryfallCard>();

                var allScryfallCards = ScryfallCards.FindAll().ToArray().GroupBy(g => g.Set).ToArray();
                var remaining = allScryfallCards.Length;

                var cardsToUpdate = new List<DetailedMagicCard>();

                var allExistingMagicCards = MagicCards.FindAll().GroupBy(c => c.SetCode).ToArray();

                Log.Debug($"Updating Scryfall cards for {remaining} sets");
                foreach (var scryfallCards in allScryfallCards)
                {
                    var key = scryfallCards.Key;
                    //// var foundMkmCards = MagicCards.Query().Where(c => c.SetCode == key).ToList();
                    var foundMkmCards = allExistingMagicCards.FirstOrDefault(k => k.Key == key)?.ToArray() ?? new DetailedMagicCard[0];

                    indexedSetData.TryGetValue(scryfallCards.Key, out lastSet);

                    var setCardsByName = foundMkmCards.GroupBy(c => c.NameEn).ToArray();

                    // Now go through all cards of that set
                    foreach (var scryfallCard in scryfallCards)
                    {
                        var foundDetail = setCardsByName
                            .FirstOrDefault(c => c.Key == scryfallCard.Name)
                            ?.ToArray() ?? new DetailedMagicCard[0];

                        DetailedMagicCard found = null;
                        if (!foundDetail.Any())
                        {
                            found = new DetailedMagicCard();
                            cardsToInsert.Add(found);
                        }
                        else if (foundMkmCards.Count() > 1)
                        {
                            // Select the correct one
                            nonUniqueCards.Add(scryfallCard);

                            continue;
                        }
                        else
                        {
                            found = foundDetail.First();
                            cardsToUpdate.Add(found);
                        }

                        found?.UpdateFromScryfall(scryfallCard, lastSet);
                    }
                }

                Log.Debug($"Inserting {cardsToInsert.Count} new Scryfall cards");
                MagicCards.InsertBulk(cardsToInsert);

                Log.Debug($"Updating {cardsToUpdate.Count} changed Scryfall cards");
                MagicCards.Update(cardsToUpdate);

                cardsToUpdate.Clear();
                cardsToInsert.Clear();

                EnsureMagicCardsIndex();

                // Now handle differnet arts of the same card
                if (nonUniqueCards.Any())
                {
                    var grouped = nonUniqueCards.GroupBy(c => $"{c.Set}_{c.Name}");
                    remaining = grouped.Count();

                    var allScryfallCardsByKey = ScryfallCards.FindAll()
                        .GroupBy(c => $"{c.Set}_{c.Name}")
                        .ToArray();

                    var allCardsBySet = MagicCards.FindAll().GroupBy(c => c.SetCode).ToArray();

                    DetailedMagicCard[] setCards = null;
                    cardsToUpdate = new List<DetailedMagicCard>();

                    foreach (var group in grouped)
                    {
                        --remaining;
                        var first = group.First();

                        // get all these cards from actual Scryfall local DB first
                        ////var scryfallCards = ScryfallCards.Query().Where(c => c.Set == first.Set && c.Name == first.Name).OrderBy(c => c.CollectorNumber).ToArray();
                        var key = $"{first.Set}_{first.Name}";
                        var scryfallCards = allScryfallCardsByKey.FirstOrDefault(f => f.Key == key).ToArray();

                        if (lastSet?.SetCodeScryfall != first.Set)
                        {
                            setCards = allCardsBySet
                                .FirstOrDefault(k => k.Key == first.Set)
                                ?.Select(c => c)
                                ?.ToArray() ?? new DetailedMagicCard[0];

                            indexedSetData.TryGetValue(first.Set, out lastSet);
                        }

                        var existingCards = setCards
                            .Where(c => c.NameEn == first.Name)
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

                        cardsToUpdate.AddRange(existingCards);
                    }

                    nonUniqueCards.Clear();
                    MagicCards.Update(cardsToUpdate);

                    EnsureMagicCardsIndex();
                }

                Log.Debug($"{nameof(RebuildDetailedDatabase)} - Done rebuilding Scryfall card data...");
            }
            finally
            {
                _cardDatabase.Pragma("CHECKPOINT", 1000);
                _scryfallDatabase.Pragma("CHECKPOINT", 1000);
                _mkmDatabase.Pragma("CHECKPOINT", 1000);
            }
        }

        internal void RebuildSetData()
        {
            // Set data
            MagicSets.DeleteAll();

            _detailedDatabaseBuilder.BuildMkmSetData();
            _detailedDatabaseBuilder.BuildScryfallSetData();
        }

        internal void EnsureSetIndex()
        {
            Log.Debug($"Rebuilding set index...");
            MagicSets.EnsureIndex(s => s.SetNameScryfall);
            MagicSets.EnsureIndex(s => s.SetNameMkm);
            MagicSets.EnsureIndex(s => s.SetCodeScryfall);
            MagicSets.EnsureIndex(s => s.SetCodeMkm);
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

        internal void EnsureMagicCardsIndex()
        {
            MagicCards.EnsureIndex(c => c.MkmId);
            MagicCards.EnsureIndex(c => c.ScryfallId);
            MagicCards.EnsureIndex(c => c.NameEn);
            MagicCards.EnsureIndex(c => c.SetCode);
            MagicCards.EnsureIndex(c => c.SetName);
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