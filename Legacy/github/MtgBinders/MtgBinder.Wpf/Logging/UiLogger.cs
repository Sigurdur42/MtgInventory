using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.Logging
{
    public class UiLogger : ILogger
    {
        private readonly string _category;

        static UiLogger()
        {
            UiCallbacks = new List<Action<DateTime, LogLevel, string, string, Exception>>();
        }

        public UiLogger(string categoryName)
        {
            _category = categoryName;
        }

        public static List<Action<DateTime, LogLevel, string, string, Exception>> UiCallbacks { get; private set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // TODO: Log into UI
            var timeStamp = DateTime.Now;

            var formatted = formatter(state, exception);
            foreach (var callback in UiCallbacks)
            {
                callback?.Invoke(timeStamp, logLevel, _category, formatted, exception);
            }
        }
    }
}