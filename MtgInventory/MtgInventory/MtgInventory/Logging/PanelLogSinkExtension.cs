using Serilog;
using Serilog.Configuration;
using System;

namespace MtgInventory.Logging
{
    public static class PanelLogSinkExtension
    {
        public static LoggerConfiguration PanelLogSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new PanelLogSink(formatProvider));
        }
    }

    
}