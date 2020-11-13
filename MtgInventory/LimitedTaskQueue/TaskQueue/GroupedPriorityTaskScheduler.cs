using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TaskQueue
{
    public class GroupedPriorityTaskScheduler
    {
        private readonly ILogger<GroupedPriorityTaskScheduler> _logger;


        public GroupedPriorityTaskScheduler(ILogger<GroupedPriorityTaskScheduler> logger)
        {
            _logger = logger;
        }

        // TODO: Configure
        // TODO: Add max degree

        public void Enqueue(
            TaskGroupPriority groupPriority,
            Action taskAction)
        {
            throw new NotImplementedException("TODO: Implement this");
        }

        public void CancelGroup(
            string groupName)
        {
            throw new NotImplementedException("TODO: Implement this");
        }

        public void CancelAll()
        {
            throw new NotImplementedException("TODO: Implement this");
        }
    }
}
