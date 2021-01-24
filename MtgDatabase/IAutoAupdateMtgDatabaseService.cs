using System;

namespace MtgDatabase
{
    public interface IAutoAupdateMtgDatabaseService
    {
        bool IsRunning { get; }

        event EventHandler UpdateFinished;
        event EventHandler UpdateStarted;

        void Start();
        void Stop();
    }
}