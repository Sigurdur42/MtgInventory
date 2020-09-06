using Avalonia.Threading;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;

namespace MtgInventory.Logging
{
    public class PanelLogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public PanelLogSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            Instance = this;
        }

        public static PanelLogSink Instance { get; private set; }

        public ObservableCollection<LogMessage> LogMessages { get; } = new ObservableCollection<LogMessage>();

        public int MaxLines { get; set; } = 30;

        public void Emit(LogEvent logEvent)
        {
            var dispatcher = Dispatcher.UIThread;

            dispatcher.Post(() =>
            {
                while (LogMessages.Count >= MaxLines)
                {
                    LogMessages.RemoveAt(0);
                }

                var levelString = logEvent.Level.ToString();
                var message = logEvent.RenderMessage(_formatProvider);
                LogMessages.Add(new LogMessage
                {
                    LogLevel = levelString.Substring(0, Math.Min(3, levelString.Length)).ToUpperInvariant(),
                    Message = message
                });
            });
        }
    }
}