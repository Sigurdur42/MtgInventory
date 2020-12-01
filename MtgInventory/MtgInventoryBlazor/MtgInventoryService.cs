using System;
using Blazored.Toast.Services;
using Microsoft.Extensions.Logging;
using MtgDatabase;

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
        private readonly IToastService _toastService;

        public MtgInventoryService(
            IMtgDatabaseService mtgDatabaseService,
            ILogger<MtgInventoryService> logger,
            IToastService toastService)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _logger = logger;
            _toastService = toastService;
        }

        public string Dummy => "My hello world";

        public event EventHandler DatabaseInitialised;
        public event EventHandler<RequestToastToDisplayEventArgs> RequestToastToDisplay;
        public bool IsDatabaseInitialized { get; private set; }

        public void Test()
        {
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Success, Header = "Init DB", Message = "Finished DB init"
                });
        }

        #region Toast Messages

        public void RequestToastError(string message, string header)
        {
            _logger.Log(LogLevel.Error, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Error, Header = header, Message = message
                });
        }

        public void RequestToastWarning(string message, string header)
        {
            _logger.Log(LogLevel.Warning, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Warning, Header = header, Message = message
                });
        }

        public void RequestToastSuccess(string message, string header)
        {
            _logger.Log(LogLevel.Information, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Success, Header = header, Message = message
                });
        }

        public void RequestToastInfo(string message, string header)
        {
            _logger.Log(LogLevel.Information, $"{header}: {message}");
            RequestToastToDisplay?.Invoke(this,
                new RequestToastToDisplayEventArgs()
                {
                    Category = ToastCategory.Info, Header = header, Message = message
                });
        }

        #endregion

        public void CreateDatabase()
        {
            try
            {
                _logger.LogInformation($"Starting database init...");
                _mtgDatabaseService.CreateDatabase(false, false);
            }
            finally
            {
                IsDatabaseInitialized = true;
                DatabaseInitialised?.Invoke(this, EventArgs.Empty);

                RequestToastInfo($"Finished database init...", "DB Init");
            }
        }
    }
}