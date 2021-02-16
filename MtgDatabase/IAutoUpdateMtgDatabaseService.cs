using System;

namespace MtgDatabase
{
    public interface IAutoUpdateMtgDatabaseService
    {
        bool IsRunning { get; }

        event EventHandler UpdateFinished;
        event EventHandler UpdateStarted;
        event EventHandler<int> UpdateProgress;

        void Start();
        void Stop(bool waitUntilStopped);
    }
}