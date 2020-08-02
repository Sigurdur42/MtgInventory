using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.Logging
{
    public class UiLoggingProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new UiLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}