using System;

namespace TaskQueue
{
    public class PriorityTaskAction
    {
        public PriorityTaskAction(
            Action actionToExecute,
            string priority)
        {
            ActionToExecute = actionToExecute;
            Priority = priority?.ToUpperInvariant() ?? "";
        }

        public Action ActionToExecute { get; }
        public string Priority { get; }
    }
}