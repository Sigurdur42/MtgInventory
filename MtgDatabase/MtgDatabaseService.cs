using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Database;
using MtgDatabase.Decks;
using MtgDatabase.Models;
using MtgDatabase.Scryfall;
using ScryfallApiServices;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IQueryableCardsProvider, IDisposable
    {
        event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding;

        bool IsRebuilding { get; }

        void Configure(DirectoryInfo folder, ScryfallConfiguration configuration, int downloadCardBatchSize);

        Task RefreshLocalDatabaseAsync(IProgress<int> progress);

        Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData);

        SetInfo[] GetAllSets();

        DatabaseSummary GetDatabaseSummary();

        Task<DeckReaderResult> ReadDeck(string name, string deckContent);
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly Database.MtgDatabase _database;
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly IScryfallService _scryfallService;
        private readonly IMirrorScryfallDatabase _mirrorScryfallDatabase;
        private readonly ITextDeckReader _deckReader;
        private readonly object _sync = new object();

        private bool _isRebuilding;

        private ScryfallConfiguration? _scryfallConfiguration;

        private int _downloadCardBachSize = 100;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IScryfallService scryfallService,
            IMirrorScryfallDatabase mirrorScryfallDatabase,
            ITextDeckReader deckReader)
        {
            _logger = logger;
            _database = database;
            _scryfallService = scryfallService;
            _mirrorScryfallDatabase = mirrorScryfallDatabase;
            _deckReader = deckReader;
            _mirrorScryfallDatabase.CardBatchDownloaded += OnMirrorScryfallCardsDownloaded;
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

        public ILiteCollection<QueryableMagicCard>? Cards => _database?.Cards;

        public async Task RefreshLocalDatabaseAsync(IProgress<int> progress)
        {
            IsRebuilding = true;
            try
            {
                _scryfallService.RefreshLocalMirror(true, true);

                await _mirrorScryfallDatabase.DownloadDatabase(_downloadCardBachSize, progress);
            }
            finally
            {
                IsRebuilding = false;
            }
        }

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
                NumberOfCardsDe = _database.Cards?.Query()?.Where(c => "DE".Equals(c.Language, StringComparison.InvariantCultureIgnoreCase))?.Count() ?? 0,
                NumberOfCardsEn = _database.Cards?.Query()?.Where(c => "EN".Equals(c.Language, StringComparison.InvariantCultureIgnoreCase)).Count() ?? 0,
                NumberOfCardsNoLanguage = _database.Cards?.Query()?.Where(c => string.IsNullOrEmpty(c.Language)).Count() ?? 0,
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
                        ResultSortOrder.ByName => query?.ToArray()?.OrderBy(c => c.Name)?.ThenBy(c => c.SetCode)?.ThenBy(c => c.CollectorNumber)?.ToArray(),
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

        private void OnMirrorScryfallCardsDownloaded(object sender, DownloadedCardsEventArgs e)
        {
            var supportedLanguages = e.DownloadedCards
                ?.Where(c => string.IsNullOrWhiteSpace(c.Lang) || "EN".Equals(c.Lang, StringComparison.InvariantCultureIgnoreCase) || "DE".Equals(c.Lang, StringComparison.InvariantCultureIgnoreCase))
                ?? Array.Empty<ScryfallJsonCard>();

            RebuildCardsFromScryfall(supportedLanguages);
        }

        private void RebuildCardsFromScryfall(IEnumerable<ScryfallJsonCard> allCards)
        {
            IsRebuilding = true;
            try
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
                    $"Inserted: {cardsToInsert.Count}, Updated: {cardsToUpdate.Count} in {stopwatch.Elapsed}");
            }
            finally
            {
                IsRebuilding = false;
            }
        }

        public async Task<DeckReaderResult> ReadDeck(string name, string deckContent)
        {
            return await Task.Run(() =>
            {
                var result = _deckReader.ReadDeck(deckName: name, deckContent: deckContent);

                return result;
            }
            );
        }
    }
}