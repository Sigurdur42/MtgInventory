using System;

namespace MtgBinder.Domain.Tools
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(string action, int range)
        {
            Action = action;
            Range = range;
        }

        public string Action { get; set; }
        public int CurrentStep { get; set; }
        public int Range { get; set; }

        public bool IsRunning { get; set; }
    }
}