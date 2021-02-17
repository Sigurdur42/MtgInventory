using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MtgDatabase.Models;
using MtgDatabase.MtgJson;

namespace MtgDatabase
{
    public class AutoUpdateMtgDatabaseService : IAutoUpdateMtgDatabaseService, IProgress<int>
    {
        public readonly ManualResetEventSlim _stopRequested = new ManualResetEventSlim();

        private readonly object _sync = new object();

        private readonly IMtgDatabaseService _databaseService;

        private readonly Database.MtgDatabase _database;

        private readonly IMirrorMtgJson _mirrorMtgJson;

        private readonly ILogger<AutoUpdateMtgDatabaseService> _logger;

        private bool _isRunning = false;

        public AutoUpdateMtgDatabaseService(
            IMtgDatabaseService databaseService,
            Database.MtgDatabase database,
            IMirrorMtgJson mirrorMtgJson,
            ILogger<AutoUpdateMtgDatabaseService> logger)
        {
            _databaseService = databaseService;
            _database = database;
            _mirrorMtgJson = mirrorMtgJson;
            _logger = logger;
        }

        public event EventHandler UpdateStarted = (sender, args) => { };

        public event EventHandler<int> UpdateProgress = (sender, args) => { };

        public event EventHandler UpdateFinished = (sender, args) => { };

        public bool IsRunning
        {
            get
            {
                lock (_sync)
                {
                    return _isRunning;
                }
            }

            private set
            {
                lock (_sync)
                {
                    _isRunning = value;
                }
            }
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            _stopRequested.Reset();
            Task.Factory.StartNew(InternalRunner);
        }

        public void Stop(bool waitUntilStopped)
        {
            _stopRequested.Set();

            if (waitUntilStopped)
            {
                while (IsRunning)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void Report(int value) => UpdateProgress?.Invoke(this, value);

        private void InternalRunner()
        {
            IsRunning = true;
            try
            {
                do
                {
                    if (_stopRequested.Wait(0))
                    {
                        break;
                    }

                    var isDbEmpty = (_database.Cards?.Count() ?? 0) == 0;
                    var anyUpdateRequired = _mirrorMtgJson.AreCardsOutdated || _mirrorMtgJson.IsPriceOutdated || isDbEmpty;

                    if (anyUpdateRequired)
                    {
                        UpdateStarted.Invoke(this, EventArgs.Empty);
                    }
                    try
                    {
                        IList<QueryableMagicCard> cards = new List<QueryableMagicCard>();
                        if (_mirrorMtgJson.AreCardsOutdated || isDbEmpty)
                        {
                            _logger.LogInformation($"Card data is outdated - starting update now...");

                            cards = _mirrorMtgJson.DownloadDatabase(true)
                                .GetAwaiter()
                                .GetResult();
                        }

                        if (anyUpdateRequired)
                        {
                            _logger.LogInformation($"Price data is outdated - starting update now...");

                            if (!cards.Any())
                            {
                                // Need to load all cards first
                                cards = _database.Cards?.FindAll().ToList() ?? new List<QueryableMagicCard>();
                            }

                            cards = _mirrorMtgJson.UpdatePriceData(cards, true)
                                .GetAwaiter()
                                .GetResult();
                        }

                        if (cards.Any())
                        {
                            _logger.LogInformation($"Writing updated cards now");

                            _database.InsertOrUpdate(cards);
                        }
                    }
                    finally
                    {
                        if (anyUpdateRequired)
                        {
                            UpdateFinished.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                while (_stopRequested.Wait(new TimeSpan(1, 0, 0)));
            }
            catch (Exception error)
            {
                _logger.LogError($"Error updating database...", error);
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}