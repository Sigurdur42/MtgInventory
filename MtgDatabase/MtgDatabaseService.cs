using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Cache;
using MtgDatabase.DatabaseDecks;
using MtgDatabase.Decks;
using MtgDatabase.Models;
using MtgDatabase.MtgJson;

namespace MtgDatabase
{
    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly Database.MtgDatabase _database;
        private readonly ITextDeckReader _deckReader;
        private readonly IImageCache _imageCache;
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly IMirrorMtgJson _mirrorMtgJson;
        private readonly object _sync = new object();

        private bool _isRebuilding;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IMirrorMtgJson mirrorMtgJson,
            ITextDeckReader deckReader,
            IImageCache imageCache)
        {
            _logger = logger;
            _database = database;
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
            DirectoryInfo folder)
        {
            _logger.LogInformation(
                $"Configuring {nameof(Database.MtgDatabase)} in {folder.FullName}");
            _database.Configure(folder);
        }

        public void Dispose() => _database?.ShutDown();

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
    }
}