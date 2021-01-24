using System;

namespace MtgDatabase
{
    public interface IAutoAupdateMtgDatabaseService
    {
        bool IsRunning { get; }

        event EventHandler UpdateFinished;
        event EventHandler UpdateStarted;
        event EventHandler<int> UpdateProgress;

        void Start();
        void Stop();
    }
}