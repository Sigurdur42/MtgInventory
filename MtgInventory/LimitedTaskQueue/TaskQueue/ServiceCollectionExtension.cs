using Microsoft.Extensions.DependencyInjection;

namespace TaskQueue
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddGroupedScheduler(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<PriorityTaskQueue>();
            serviceCollection.AddSingleton<IGroupedPriorityTaskScheduler, GroupedPriorityTaskScheduler>();

            return serviceCollection;
        }
    }
}