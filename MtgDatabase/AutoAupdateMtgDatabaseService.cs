using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MtgDatabase
{
    public class AutoAupdateMtgDatabaseService : IAutoAupdateMtgDatabaseService, IProgress<int>
    {
        public AutoAupdateMtgDatabaseService(
            IMtgDatabaseService databaseService,
            ILogger<AutoAupdateMtgDatabaseService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public event EventHandler UpdateStarted = (sender, args) => { };
        public event EventHandler<int> UpdateProgress = (sender, args) => { };

        public event EventHandler UpdateFinished = (sender, args) => { };

        public readonly ManualResetEventSlim _stopRequested = new ManualResetEventSlim();

        private readonly object _sync = new object();
        private readonly IMtgDatabaseService _databaseService;
        private readonly ILogger<AutoAupdateMtgDatabaseService> _logger;
        private bool _isRunning = false;

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

        public void Stop()
        {
            _stopRequested.Set();
        }

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

                    //var summary = _databaseService.GetDatabaseSummary();
                    //var isOutdated = summary.LastUpdated.AddDays(1).Date < DateTime.Now.Date;
                    //if (isOutdated)
                    //{
                    //    _logger.LogInformation($"Database is outdated - starting update now...");
                        UpdateStarted.Invoke(this, EventArgs.Empty);
                        try
                        {
                            _databaseService.RefreshLocalDatabaseAsync(this, false).GetAwaiter().GetResult();
                        }
                        finally
                        {
                            UpdateFinished.Invoke(this, EventArgs.Empty);
                        }

                    // }
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

        public void Report(int value) => UpdateProgress?.Invoke(this, value);
    }
}
