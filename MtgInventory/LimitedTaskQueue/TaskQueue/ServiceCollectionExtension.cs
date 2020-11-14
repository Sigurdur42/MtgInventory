using Microsoft.Extensions.DependencyInjection;
using TaskQueue;

namespace MtgInventory.Service
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