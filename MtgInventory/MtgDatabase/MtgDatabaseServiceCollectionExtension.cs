using Microsoft.Extensions.DependencyInjection;
using ScryfallApiServices;

namespace MtgDatabase
{
    public static class MtgDatabaseServiceCollectionExtension
    {
        public static IServiceCollection AddMtgDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScryfallApi();

            serviceCollection.AddSingleton<IMtgDatabaseService, MtgDatabaseService>();
            serviceCollection.AddSingleton<Database.MtgDatabase>();
            serviceCollection.AddSingleton<IMkmMapper, MkmMapper>();
            
            return serviceCollection;
        }
    }
}