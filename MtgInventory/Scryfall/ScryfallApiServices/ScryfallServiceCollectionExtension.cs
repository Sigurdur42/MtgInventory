using Microsoft.Extensions.DependencyInjection;

namespace ScryfallApiServices
{
    public static class ScryfallServiceCollectionExtension
    {
        public static IServiceCollection AddScryfallApi(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IScryfallApiCallStatistic, ScryfallApiCallStatistic>();
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();

            return serviceCollection;
        }
    }
}