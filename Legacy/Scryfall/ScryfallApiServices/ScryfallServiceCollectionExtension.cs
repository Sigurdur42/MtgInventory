using Microsoft.Extensions.DependencyInjection;
using ScryfallApiServices.Database;

namespace ScryfallApiServices
{
    public static class ScryfallServiceCollectionExtension
    {
        public static IServiceCollection AddScryfallApi(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IScryfallApiCallStatistic, ScryfallApiCallStatistic>();
            serviceCollection.AddSingleton<IScryfallDatabase, ScryfallDatabase>();
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();
            serviceCollection.AddSingleton<IConfigurationFolderProvider, ConfigurationFolderProvider>();

            return serviceCollection;
        }
    }
}