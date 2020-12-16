using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

namespace MtgInventory.Logging
{
    public class PanelLogSinkLogger : ILogger
    {
        private readonly string _category;

        private readonly ConcurrentBag<LogMessage> _newMessages = new ConcurrentBag<LogMessage>();

        private Task? _currentBackgroundThread;

        static PanelLogSinkLogger()
        {
        }

        public PanelLogSinkLogger(string categoryName)
        {
            _category = categoryName;
        }

        public ObservableCollection<LogMessage> LogMessages { get; } = new ObservableCollection<LogMessage>();

        public int MaxLines { get; set; } = 30;

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

            var levelString = logLevel.ToString();
            var formatted = formatter(state, exception);
            var logMessage = new LogMessage
            {
                LogLevel = levelString.Substring(0, Math.Min(3, levelString.Length)).ToUpperInvariant(),
                Message = formatted
            };

            _newMessages.Add(logMessage);

            if (((_currentBackgroundThread?.IsCompleted) ?? false)
                && _newMessages.Count > 1)
            {
                return;
            }

            _currentBackgroundThread = Task.Factory.StartNew(() =>
            {
                if (_newMessages.IsEmpty)
                {
                    return;
                }

                var dispatcher = Dispatcher.UIThread;

                dispatcher.Post(() =>
                {
                    while (_newMessages.TryTake(out var msg))
                    {
                        while (LogMessages.Count >= MaxLines)
                        {
                            LogMessages.RemoveAt(0);
                        }

                        LogMessages.Add(msg);
                    }
                });
            });
        }
    }
}