using Microsoft.Extensions.DependencyInjection;
using MtgDatabase.Scryfall;
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

            serviceCollection.AddSingleton<IMirrorScryfallDatabase, MirrorScryfallDatabase>();
            return serviceCollection;
        }
    }
}