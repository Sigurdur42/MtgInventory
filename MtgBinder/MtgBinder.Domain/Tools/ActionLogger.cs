using System;
using System.Diagnostics;
using System.Text;
using Serilog;

namespace MtgBinder.Domain.Tools
{
    public sealed class ActionLogger : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        public ActionLogger(
            string action,
            string description)
        {
            _stopwatch = Stopwatch.StartNew();

            var builder = new StringBuilder();
            builder.Append(action);
            builder.Append(":");
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.Append(" ");
                builder.Append(description);
            }

            Prefix = builder.ToString();

            Log.Information($"{Prefix} - Starting...");
        }

        public string Result { get; set; }
        public string Prefix { get; }

        public void Information(string logMessage)
        {
            Log.Information($"{Prefix} - {logMessage}");
        }

        public void Dispose()
        {
            _stopwatch.Stop();

            var builder = new StringBuilder(Prefix);

            builder.Append($" took {_stopwatch.Elapsed}");

            if (!string.IsNullOrWhiteSpace(Result))
            {
                builder.Append(" - ");
                builder.Append(Result);
            }

            Log.Information(builder.ToString());
        }
    }
}