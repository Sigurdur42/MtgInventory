using System;

namespace MtgDatabase
{
    public class DatabaseRebuildingEventArgs : EventArgs
    {
        public bool RebuildingStarted { get; internal set; }
    }
}