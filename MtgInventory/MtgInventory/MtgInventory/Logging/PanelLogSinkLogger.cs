using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace MtgInventory.Logging
{
    public class PanelLogSinkLogger : ILogger
    {
        private readonly string _category;

        static PanelLogSinkLogger()
        {
        }

        public PanelLogSinkLogger(string categoryName)
        {
            _category = categoryName;
        }

        public ObservableCollection<LogMessage> LogMessages { get; } = new ObservableCollection<LogMessage>();

        public int MaxLines { get; set; } = 50;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var timeStamp = DateTime.Now;

            var formatted = formatter(state, exception);

            var dispatcher = Dispatcher.UIThread;

            dispatcher.Post(() =>
            {
                while (LogMessages.Count >= MaxLines)
                {
                    LogMessages.RemoveAt(0);
                }

                var levelString = logLevel.ToString();
                var message = formatted;

                LogMessages.Add(new LogMessage
                {
                    LogLevel = levelString.Substring(0, Math.Min(3, levelString.Length)).ToUpperInvariant(),
                    Message = message
                });
            });
        }
    }
}