using Microsoft.Extensions.Logging;

namespace MtgInventory.Logging
{
    public class PanelLogSinkProvider : ILoggerProvider
    {
        public static PanelLogSinkLogger LoggerInstance { get; } = new PanelLogSinkLogger("");

        public ILogger CreateLogger(string categoryName)
        {
            return LoggerInstance;
        }

        public void Dispose()
        {
        }
    }
}