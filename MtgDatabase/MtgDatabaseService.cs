using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Database;
using MtgDatabase.Models;
using MtgDatabase.Scryfall;
using ScryfallApiServices;
using ScryfallApiServices.Models;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IQueryableCardsProvider, IDisposable
    {
        bool IsRebuilding { get; }
        void Configure(DirectoryInfo folder, ScryfallConfiguration configuration, int downloadCardBatchSize);
        Task RefreshLocalDatabaseAsync();
        // void RebuildSetData(SetInfo setInfo);
        // void DownloadRebuildSetData(SetInfo setInfo);

        Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData);
        SetInfo[] GetAllSets();

        DatabaseSummary GetDatabaseSummary();
        event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding;
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly Database.MtgDatabase _database;
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly IScryfallService _scryfallService;
        private readonly IMirrorScryfallDatabase _mirrorScryfallDatabase;

        private readonly object _sync = new object();

        private bool _isRebuilding;

        private ScryfallConfiguration? _scryfallConfiguration;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IScryfallService scryfallService,
            IMirrorScryfallDatabase mirrorScryfallDatabase)
        {
            _logger = logger;
            _database = database;
            _scryfallService = scryfallService;
            _mirrorScryfallDatabase = mirrorScryfallDatabase;
            
            _mirrorScryfallDatabase.CardBatchDownloaded += OnMirrorScryfallCardsDownloaded;
        }

        private void OnMirrorScryfallCardsDownloaded(object sender, DownloadedCardsEventArgs e)
        {
            var supportedLanguages = e.DownloadedCards
                .Where(c => string.IsNullOrWhiteSpace(c.Lang) || "EN".Equals(c.Lang, StringComparison.InvariantCultureIgnoreCase) || "DE".Equals(c.Lang, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            if (supportedLanguages.Any())
            {
                RebuildCardsFromScryfall(supportedLanguages);
            }
        }

        public event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding = (sender, args) => { };

        public bool IsRebuilding
        {
            get
            {
                lock (_sync)
                {
                    return _isRebuilding;
                }
            }

            private set
            {
                lock (_sync)
                {
                    _isRebuilding = value;
                }

                OnRebuilding?.Invoke(this, new DatabaseRebuildingEventArgs
                {
                    RebuildingStarted = value
                });
            }
        }

        public async Task RefreshLocalDatabaseAsync()
        {
            IsRebuilding = true;
            try
            {
                _scryfallService.RefreshLocalMirror(true, true);
                
                // RebuildInternalDatabase(clearMtgDatabase || clearScryfallMirror);
                await _mirrorScryfallDatabase.DownloadDatabase(_downloadCardBachSize);
            }
            finally
            {
                IsRebuilding = false;
            }
        }

        // public void RebuildSetData(SetInfo setInfo)
        // {
        //     IsRebuilding = true;
        //     try
        //     {
        //         var found = _scryfallService.ScryfallSets?.Query()?.Where(s => s.Code == setInfo.Code)?.FirstOrDefault();
        //         if (found == null)
        //         {
        //             return;
        //         }
        //
        //         var oldDate = found.UpdateDateUtc;
        //         found.UpdateDateUtc = DateTime.MinValue;
        //         _scryfallService.ScryfallSets?.Update(found);
        //         RebuildInternalDatabase(false);
        //
        //         found.UpdateDateUtc = oldDate;
        //         _scryfallService.ScryfallSets?.Update(found);
        //     }
        //     finally
        //     {
        //         IsRebuilding = false;
        //     }
        // }
        //
        // public void DownloadRebuildSetData(SetInfo setInfo)
        // {
        //     IsRebuilding = true;
        //     try
        //     {
        //         _scryfallService.MarkSetCardsAsOutdated(setInfo.Code);
        //         var cards = _scryfallService.RefreshLocalMirrorForSet(setInfo.Code);
        //
        //         RebuildCardsFromScryfall(cards);
        //     }
        //     finally
        //     {
        //         IsRebuilding = false;
        //     }
        // }

        private int _downloadCardBachSize = 100;
        
        public void Configure(
            DirectoryInfo folder, 
            ScryfallConfiguration configuration,
            int downloadCardBachSize)
        {
            _scryfallConfiguration = configuration;
            _downloadCardBachSize = downloadCardBachSize;
            
            _logger.LogInformation(
                $"Configuring {nameof(Database.MtgDatabase)} in {folder.FullName} with {Environment.NewLine}{configuration.DumpSettings()}");
            _scryfallService.Configure(folder, configuration);
            _database.Configure(folder);
        }

        public void Dispose() => _database?.ShutDown();

        public ILiteCollection<QueryableMagicCard>? Cards => _database?.Cards;

        public SetInfo[] GetAllSets() =>
            _scryfallService.ScryfallSets
                ?.FindAll()
                ?.Select(s => new SetInfo
                {
                    Code = s.Code,
                    ParentSetCode = s.ParentSetCode,
                    Name = s.Name,
                    ReleaseDate = s.ReleaseDate,
                    IsDigital = s.IsDigital,
                    SetType = s.SetType,
                    IconSvgUri = s.IconSvgUri?.AbsolutePath != null ? "https://c2.scryfall.com" + s.IconSvgUri?.AbsolutePath : "",
                    CardCount = s.card_count,
                    UpdateDateUtc = s.UpdateDateUtc
                })
                ?.ToArray()
            ?? Array.Empty<SetInfo>();

        public DatabaseSummary GetDatabaseSummary() =>
            new DatabaseSummary
            {
                LastUpdated = _scryfallService.ScryfallSets?.Query().OrderByDescending(s => s.UpdateDateUtc).FirstOrDefault()?.UpdateDateUtc ?? DateTime.MinValue,
                NumberOfCards = _database.Cards?.Count() ?? 0,
                NumberOfSets = _scryfallService?.ScryfallSets?.Count() ?? 0,
                NumberOfCardsDe = _database.Cards?.Query()?.Where(c=>"DE".Equals(c.Language, StringComparison.InvariantCultureIgnoreCase))?.Count() ?? 0,
                NumberOfCardsEn = _database.Cards?.Query()?.Where(c=>"EN".Equals(c.Language, StringComparison.InvariantCultureIgnoreCase)).Count() ?? 0,
                NumberOfCardsNoLanguage = _database.Cards?.Query()?.Where(c=>string.IsNullOrEmpty(c.Language)).Count() ?? 0,
            };

        public Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData) =>
            Task.Run(() =>
            {
                if (!(queryData?.ContainsValidSearch() ?? false))
                {
                    return Array.Empty<QueryableMagicCard>();
                }

                try
                {
                    var query = Cards?.Query();

                    if (queryData.IsToken)
                    {
                        query = query?.Where(c => c.IsToken);
                    }

                    if (queryData.IsBasicLand)
                    {
                        query = query?.Where(c => c.IsBasicLand);
                    }

                    if (!string.IsNullOrWhiteSpace(queryData.Name))
                    {
                        if (queryData.MatchExactName)
                        {
                            query = query?.Where(c =>
                                c.LocalName.Equals(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else
                        {
                            query = query?.Where(c =>
                                c.LocalName.Contains(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(queryData.SetCode))
                    {
                        query = query?.Where(c => c.SetCode.Equals(queryData.SetCode, StringComparison.InvariantCultureIgnoreCase));
                    }

                    var result = queryData.ResultSortOrder switch
                    {
                        ResultSortOrder.ByName => query?.OrderBy(c => c.Name)?.ToArray(),
                        ResultSortOrder.ByCollectorNumber => query?.OrderBy(c => c.CollectorNumber)?.ToArray(),
                        _ => query?.ToArray()
                    };

                    return result ?? Array.Empty<QueryableMagicCard>();
                }
                catch (Exception error)
                {
                    _logger.LogError($"Error running query: {error}");
                    return Array.Empty<QueryableMagicCard>();
                }
            });

        private void RebuildInternalDatabase(bool clearDatabase)
        {
            if (clearDatabase)
            {
                _database.Cards?.DeleteAll();
            }

            var oldestCard =
                _database.Cards?.Query().OrderBy(c => c.UpdateDateUtc).FirstOrDefault()?.UpdateDateUtc ??
                DateTime.MinValue;
            var oldestDate = _scryfallConfiguration?.IsCardOutdated(oldestCard) ?? false;
            if (!clearDatabase && oldestDate)
            {
                // Cards are up to date - skip this
                _logger.LogTrace("All cards are up to date - skip rebuild");
                return;
            }

            _logger.LogTrace("Retrieving all cards from cache...");
            var stopwatch = Stopwatch.StartNew();
            var allCards = _scryfallService.ScryfallCards?.FindAll().ToArray() ?? Array.Empty<ScryfallCard>();
            _logger.LogTrace($"Retrieving all cards from cache took {stopwatch.Elapsed} for {allCards.Length} cards");

            stopwatch.Restart();
            // var groupedByName = allCards.GroupBy(c => c.Name).ToArray();
            // _logger.LogTrace($"Found {groupedByName.Length} distinct cards in {stopwatch.Elapsed}");
            // stopwatch.Restart();

            // TEST ONLY
            // var groupedTypelines = allCards
            //     .GroupBy(c => c.TypeLine)
            //     .OrderBy(c=>c.Key)
            //     .Select(c=>c.Key)
            //     .ToArray();
            // var dummy = string.Join(Environment.NewLine, groupedTypelines);
            // File.WriteAllText(@"C:\temp\typelines.txt", dummy);

            RebuildCardsFromScryfall(allCards);
        }

        private void RebuildCardsFromScryfall(ScryfallCard[] allCards)
        {
            var stopwatch = Stopwatch.StartNew();
            var cardFactory = new QueryableMagicCardFactory();
            var cardsToInsert = new List<QueryableMagicCard>();
            var cardsToUpdate = new List<QueryableMagicCard>();
            foreach (var group in allCards)
            {
                var card = cardFactory.Create(group);
                var found = _database.Cards?.Query()?.Where(c => c.UniqueId == card.UniqueId)?.FirstOrDefault();
                if (found == null)
                {
                    cardsToInsert.Add(card);
                }
                else
                {
                    cardsToUpdate.Add(card);
                }
            }

            if (cardsToInsert.Any())
            {
                _database.Cards?.InsertBulk(cardsToInsert);
            }

            if (cardsToUpdate.Any())
            {
                _database.Cards?.Update(cardsToUpdate);
            }

            if (cardsToInsert.Any() || cardsToUpdate.Any())
            {
                _database.EnsureIndex();
            }

            stopwatch.Stop();

            _logger.LogTrace(
                $"Inserted: {cardsToInsert.Count()}, Updated: {cardsToUpdate.Count()} in {stopwatch.Elapsed}");
        }
        
        private void RebuildCardsFromScryfall(ScryfallJsonCard[] allCards)
        {
            var stopwatch = Stopwatch.StartNew();
            var cardFactory = new QueryableMagicCardFactory();
            var cardsToInsert = new List<QueryableMagicCard>();
            var cardsToUpdate = new List<QueryableMagicCard>();
            foreach (var group in allCards)
            {
                var card = cardFactory.Create(group);
                var found = _database.Cards?.Query()?.Where(c => c.UniqueId == card.UniqueId)?.FirstOrDefault();
                if (found == null)
                {
                    cardsToInsert.Add(card);
                }
                else
                {
                    cardsToUpdate.Add(card);
                }
            }

            if (cardsToInsert.Any())
            {
                _database.Cards?.InsertBulk(cardsToInsert);
            }

            if (cardsToUpdate.Any())
            {
                _database.Cards?.Update(cardsToUpdate);
            }

            if (cardsToInsert.Any() || cardsToUpdate.Any())
            {
                _database.EnsureIndex();
            }

            stopwatch.Stop();

            _logger.LogTrace(
                $"Inserted: {cardsToInsert.Count()}, Updated: {cardsToUpdate.Count()} in {stopwatch.Elapsed}");
        }
    }
}