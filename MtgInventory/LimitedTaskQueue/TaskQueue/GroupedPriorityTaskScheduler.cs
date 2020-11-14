using System;
using Microsoft.Extensions.Logging;

namespace TaskQueue
{
    public interface IGroupedPriorityTaskScheduler
    {
        void CancelAll();

        void CancelGroup(
            string groupName);

        void Enqueue(
            TaskGroupPriority groupPriority,
            Action taskAction);
    }

    public class GroupedPriorityTaskScheduler : IGroupedPriorityTaskScheduler
    {
        private readonly ILogger<GroupedPriorityTaskScheduler> _logger;
        private readonly PriorityTaskQueue _taskQueue;

        public GroupedPriorityTaskScheduler(
            ILogger<GroupedPriorityTaskScheduler> logger,
            PriorityTaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        // TODO: Configure
        // TODO: Add max degree

        public void CancelAll()
        {
            throw new NotImplementedException("TODO: Implement this");
        }

        public void CancelGroup(
            string groupName)
        {
            throw new NotImplementedException("TODO: Implement this");
        }

        public void Enqueue(
            TaskGroupPriority groupPriority,
            Action taskAction)
        {
            _taskQueue.Enqueue(new PriorityTaskAction(taskAction, groupPriority.ToString()));
        }
    }
}