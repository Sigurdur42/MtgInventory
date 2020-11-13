using System.Collections.Generic;
using System.Linq;

namespace TaskQueue
{
    internal class PriorityTaskQueue
    {
        private readonly SortedSet<PriorityTaskAction> _actions = new SortedSet<PriorityTaskAction>(new PriorityTaskActionComparer());
        private readonly object _sync = new object();

        private readonly LimitedConcurrencyLevelTaskScheduler _taskScheduler;

        public PriorityTaskQueue(int maxDegreeOfParallelism)
        {
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
        }

        public void Enqueue(PriorityTaskAction action)
        {
            lock (_sync)
            {
                _actions.Add(action);
            }

            // TODO: Start queue on demand?
        }

        public PriorityTaskAction? TryDequeue()
        {
            lock (_sync)
            {
                var found = _actions.FirstOrDefault();
                if (found == null)
                {
                    return found;
                }

                _actions.Remove(found);
                return found;
            }
        }

        public int QueueLength
        {
            get
            {
                lock (_sync)
                {
                    return _actions.Count;
                }
            }
        }

        public string[] GetQueuedItemIds()
        {
            lock (_sync)
            {
                return _actions.Select(_ => _.Priority).ToArray();
            }
        }
    }
}