using Microsoft.Extensions.DependencyInjection;

namespace MkmApi
{
    public static class MkmServiceCollectionExtension
    {
        public static IServiceCollection AddMkmApi(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IApiCallStatistic, ApiCallStatistics>();
            serviceCollection.AddSingleton<IMkmRequest, MkmRequest>();

            return serviceCollection;
        }
    }
}