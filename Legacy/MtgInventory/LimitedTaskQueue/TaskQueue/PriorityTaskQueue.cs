using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TaskQueue
{
    public class PriorityTaskQueue
    {
        private readonly SortedSet<PriorityTaskAction> _actions = new SortedSet<PriorityTaskAction>(new PriorityTaskActionComparer());
        private readonly ILogger<PriorityTaskQueue> _logger;
        private readonly object _sync = new object();
        private int _runningTasks = 0;

        public PriorityTaskQueue(
            ILogger<PriorityTaskQueue> logger)
        {
            _logger = logger;
            MaxDegreeOfParallelism = Environment.ProcessorCount;
        }

        public int MaxDegreeOfParallelism { get; set; }

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

        public int RunningTasks
        {
            get
            {
                lock (_sync)
                {
                    return _runningTasks;
                }
            }
        }

        public void Enqueue(PriorityTaskAction action)
        {
            lock (_sync)
            {
                _actions.Add(action);
            }

            RunTasks();
        }

        public string[] GetQueuedItemIds()
        {
            lock (_sync)
            {
                return _actions.Select(_ => _.Priority).ToArray();
            }
        }

        private void RunSingleTask(PriorityTaskAction action)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogTrace($"Task '{action.Priority}' is starting now");

                action.ActionToExecute.Invoke();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in action {action.Priority}: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"Task '{action.Priority}' finished in {stopwatch.Elapsed}");

                lock (_sync)
                {
                    _runningTasks--;
                }

                RunTasks();
            }
        }

        private void RunTasks()
        {
            while (MaxDegreeOfParallelism > RunningTasks)
            {
                if (!RunNextTask())
                {
                    break;
                }
            }

            bool RunNextTask()
            {
                PriorityTaskAction? taskToExecute = null;
                lock (_sync)
                {
                    // Loop: On all posible tasks

                    taskToExecute = TryDequeue();
                    if (taskToExecute == null)
                    {
                        _logger.LogTrace($"No more task in queue.");
                        return false;
                    }

                    _runningTasks++;
                }

                _logger.LogTrace($"Task '{taskToExecute.Priority}' is queue for execution now");
                Task.Factory.StartNew(() => RunSingleTask(taskToExecute));

                return true;
            }
        }

        private PriorityTaskAction? TryDequeue()
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
    }
}