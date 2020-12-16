using System;
using System.Collections.Generic;

namespace TaskQueue
{
    internal class PriorityTaskActionComparer : IComparer<PriorityTaskAction>
    {
        public int Compare(PriorityTaskAction x, PriorityTaskAction y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (ReferenceEquals(null, y))
            {
                return 1;
            }

            if (ReferenceEquals(null, x))
            {
                return -1;
            }

            return string.Compare(x.Priority, y.Priority, StringComparison.OrdinalIgnoreCase);
        }
    }
}