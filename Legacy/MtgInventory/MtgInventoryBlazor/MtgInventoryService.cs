using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgDatabase.Models;
using MtgInventoryBlazor.Data;

namespace MtgInventoryBlazor
{
    public interface IRequestToastMessage
    {
        void RequestToastError(string message, string header);
        void RequestToastWarning(string message, string header);
        void RequestToastSuccess(string message, string header);
        void RequestToastInfo(string message, string header);
    }

    public class MtgInventoryService : IRequestToastMessage
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly ILogger<MtgInventoryService> _logger;

        public MtgInventoryService(
            IMtgDatabaseService mtgDatabaseService,
            ILogger<MtgInventoryService> logger)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _logger = logger;
        }


        public MtgDatabaseQueryData DatabaseQueryData { get; set; } = new MtgDatabaseQueryData();

        public event EventHandler DatabaseInitialised = (sender, args) => { };
        public event EventHandler<RequestToastToDisplayEventArgs> RequestToastToDisplay = (sender, args) => { };
        public bool IsDatabaseInitialized { get; private set; }

        public void Test()
        {
            RequestToastToDisplay?.Invoke(
                this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Success,
                    Header = "Init DB",
                    Message = "Finished DB init"
                });
        }

        #region Toast Messages

        public void RequestToastError(string message, string header)
        {
            _logger.Log(LogLevel.Error, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Error,
                    Header = header,
                    Message = message
                });
        }

        public void RequestToastWarning(string message, string header)
        {
            _logger.Log(LogLevel.Warning, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Warning,
                    Header = header,
                    Message = message
                });
        }

        public void RequestToastSuccess(string message, string header)
        {
            _logger.Log(LogLevel.Information, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Success,
                    Header = header,
                    Message = message
                });
        }

        public void RequestToastInfo(string message, string header)
        {
            _logger.Log(LogLevel.Information, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Info,
                    Header = header,
                    Message = message
                });
        }

        #endregion

        public void CreateDatabase()
        {
            try
            {
                _logger.LogInformation($"Starting database init...");
                _mtgDatabaseService.RefreshLocalDatabase(false, false);
            }
            finally
            {
                IsDatabaseInitialized = true;
                DatabaseInitialised?.Invoke(this, EventArgs.Empty);

                RequestToastInfo($"Finished database init...", "DB Init");
            }
        }

        public async Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData)
        {
            RequestToastInfo("Starting card search", "Card search");

            var stopwatch = Stopwatch.StartNew();
            var task = _mtgDatabaseService.SearchCardsAsync(queryData);
            await task.ContinueWith((t) =>
            {
                stopwatch.Stop();
                RequestToastSuccess($"Finished card search with {t.Result.Length} cards in {stopwatch.Elapsed}", "Card search");
            });

            return await task;
        }

        public SetInfo[] GetAllSets() => _mtgDatabaseService.GetAllSets().OrderBy(s => s.Name).ToArray();

        public async Task RebuildSetDataAsync(SetInfo setInfo)
        {
            await Task.Run(() =>
            {
                RequestToastInfo($"Start set rebuild for {setInfo.Code}...", "Sets");
                
                _mtgDatabaseService.RebuildSetData(setInfo);
                RequestToastSuccess($"Done rebuilding for set {setInfo.Code}...", "Sets");
            });
        }
        
        public async Task DownloadRebuildSetDataAsync(SetInfo setInfo)
        {
            await Task.Run(() =>
            {
                RequestToastInfo($"Start set download rebuild for {setInfo.Code}...", "Sets");
                _mtgDatabaseService.DownloadRebuildSetData(setInfo);
                RequestToastSuccess($"Done rebuilding for set {setInfo.Code}...", "Sets");
            });
        }
    }
}