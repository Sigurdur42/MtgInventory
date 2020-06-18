using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgBinder.Domain.Tools
{
    public class AsyncProgressNotifier : IAsyncProgressNotifier, IAsyncProgress
    {
        private readonly Dictionary<string, ProgressData> _data = new Dictionary<string, ProgressData>();

        public event EventHandler<ProgressEventArgs> Starting;

        public event EventHandler<ProgressEventArgs> Finishing;

        public event EventHandler<ProgressEventArgs> Progress;

        public void Start(string action, int range)
        {
            lock (_data)
            {
                if (!_data.TryGetValue(action, out var progressData))
                {
                    progressData = new ProgressData(action, range);
                    _data.Add(action, progressData);
                }

                progressData.Steps = 0;
            }

            Starting?.Invoke(this, GetEventArgs(action));
        }

        public void NextStep(string action)
        {
            var hasFound = false;
            lock (_data)
            {
                if (_data.TryGetValue(action, out var progressData))
                {
                    hasFound = true;
                    progressData.Steps += 1;
                }
            }

            if (hasFound)
            {
                Progress?.Invoke(this, GetEventArgs(action));
            }
        }

        public void Finish(string action)
        {
            var hasFound = false;

            lock (_data)
            {
                if (_data.ContainsKey(action))
                {
                    hasFound = true;
                    _data.Remove(action);
                }
            }

            if (hasFound)
            {
                Finishing?.Invoke(this, GetEventArgs(action));
            }
        }

        private ProgressEventArgs GetEventArgs(string action)
        {
            lock (_data)
            {
                return new ProgressEventArgs(action, _data.Sum(d => d.Value.Range))
                {
                    CurrentStep = _data.Sum(d => d.Value.Steps),
                    IsRunning = _data.Any()
                };
            }
        }
    }

    internal class ProgressData
    {
        public ProgressData(string name, int range)
        {
            Name = name;
            Range = range;
        }

        public string Name { get; }
        public int Range { get; }

        public int Steps { get; set; }
    }
}