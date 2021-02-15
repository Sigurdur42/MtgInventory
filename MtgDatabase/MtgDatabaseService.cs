using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Cache;
using MtgDatabase.Database;
using MtgDatabase.DatabaseDecks;
using MtgDatabase.Decks;
using MtgDatabase.Models;
using MtgDatabase.MtgJson;
using MtgDatabase.Scryfall;
using ScryfallApiServices;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IQueryableCardsProvider, IDisposable
    {
        event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding;

        bool IsRebuilding { get; }

        void Configure(DirectoryInfo folder, ScryfallConfiguration configuration, int downloadCardBatchSize);

        SetInfo[] GetAllSets();

        DatabaseSummary GetDatabaseSummary();

        Task<DatabaseDeckReaderResult> ReadDeck(string name, string deckContent);

        Task RefreshLocalDatabaseAsync(IProgress<int>? progress, bool force);

        Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData);
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly Database.MtgDatabase _database;
        private readonly ITextDeckReader _deckReader;
        private readonly IImageCache _imageCache;
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly IMirrorMtgJson _mirrorMtgJson;
        private readonly IScryfallService _scryfallService;
        private readonly object _sync = new object();

        private int _downloadCardBachSize = 100;
        private bool _isRebuilding;

        private ScryfallConfiguration? _scryfallConfiguration;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IScryfallService scryfallService,
            IMirrorMtgJson mirrorMtgJson,
            ITextDeckReader deckReader,
            IImageCache imageCache)
        {
            _logger = logger;
            _database = database;
            _scryfallService = scryfallService;
            _mirrorMtgJson = mirrorMtgJson;
            _deckReader = deckReader;
            _imageCache = imageCache;
        }

        public event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding = (sender, args) => { };

        public ILiteCollection<QueryableMagicCard>? Cards => _database?.Cards;

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

        public async Task<DatabaseDeckReaderResult> ReadDeck(string name, string deckContent)
        {
            return await Task.Run(() =>
            {
                var result = _deckReader.ReadDeck(deckName: name, deckContent: deckContent);

                _logger.LogTrace($"Read deck with {result.Deck.GetTotalCardCount()} cards. Now matching with database...");
                var errorLines = new List<DatabaseDeckErrorLine>();
                errorLines.AddRange(result.UnreadLines.Select(c => new DatabaseDeckErrorLine
                {
                    Line = c,
                    Reason = DeckErrorLineReason.CannotParse,
                }));

                var foundCardsToCache = new List<QueryableMagicCard>();
                var databaseDeck = new DatabaseDeck()
                {
                    Name = result.Name,
                };

                foreach (var category in result.Deck.Categories)
                {
                    var deckCategory = new DatabaseDeckCategory()
                    {
                        CategoryName = category.CategoryName,
                    };

                    databaseDeck.Categories.Add(deckCategory);

                    foreach (var line in category.Lines)
                    {
                        var databaseLine = new DatabaseDeckLine
                        {
                            Quantity = line.Quantity,
                        };

                        var foundCards = _database.Cards?
                            .Query()
                            .Where(c => c.Name.Equals(line.CardName, StringComparison.InvariantCultureIgnoreCase))
                            .Where(c => c.Language == "en")
                            .ToArray();

                        if (!foundCards.Any())
                        {
                            // Card could not be found at all
                            errorLines.Add(new DatabaseDeckErrorLine()
                            {
                                Line = line.OriginalLine,
                                Reason = DeckErrorLineReason.CannotFindCardInDatabase
                            });
                        }
                        else
                        {
                            var cardWithPrice = foundCards.OrderBy(c => c.Eur ?? 100000).ToArray();
                            databaseLine.Card = cardWithPrice.First();
                            deckCategory.Lines.Add(databaseLine);

                            foundCardsToCache.Add(databaseLine.Card);
                        }
                    }
                }

                if (foundCardsToCache.Any())
                {
                    // TODO: Make this configurable
                    _imageCache.QueueForDownload(foundCardsToCache.ToArray());
                }

                return new DatabaseDeckReaderResult
                {
                    Deck = databaseDeck,
                    Name = result.Name,
                    UnreadLines = errorLines.ToArray(),
                };
            }
            );
        }

        public async Task RefreshLocalDatabaseAsync(IProgress<int>? progress, bool force)
        {
            IsRebuilding = true;
            try
            {
                // _scryfallService.RefreshLocalMirror(true, true);

                // await _mirrorScryfallDatabase.DownloadDatabase(_downloadCardBachSize, progress);
                var cards = await _mirrorMtgJson.DownloadDatabase(force);
                _database.InsertOrUpdate(cards);
            }
            finally
            {
                IsRebuilding = false;
            }
        }

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
                                c.Name.Equals(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
                        }
                        else
                        {
                            query = query?.Where(c =>
                                c.Name.Contains(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
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

                    if (result?.Any() ?? false)
                    {
                        // TODO: Make this configurable
                        _imageCache.QueueForDownload(result);
                    }

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
                ?.Where(c => c.Digital == false)
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
    }
}