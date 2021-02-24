using Microsoft.Extensions.DependencyInjection;
using MtgDatabase.Cache;
using MtgDatabase.Decks;
using MtgDatabase.MtgJson;
using MtgDatabase.Scryfall;
using MtgJson;

namespace MtgDatabase
{
    public static class MtgDatabaseServiceCollectionExtension
    {
        public static IServiceCollection AddMtgDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<GoodCitizenAutoSleep>();
            serviceCollection.AddSingleton<IMirrorScryfallDatabase, MirrorScryfallDatabase>();
            serviceCollection.AddSingleton<IMtgDatabaseService, MtgDatabaseService>();
            serviceCollection.AddSingleton<IImageCache, ImageCache>();
            serviceCollection.AddSingleton<Database.MtgDatabase>();
            serviceCollection.AddSingleton<IMkmMapper, MkmMapper>();
            serviceCollection.AddSingleton<IAutoUpdateMtgDatabaseService, AutoUpdateMtgDatabaseService>();

            serviceCollection.AddSingleton<ITextDeckReader, TextDeckReader>();
            serviceCollection.AddSingleton<IMirrorMtgJson, MirrorMtgJson>();

            serviceCollection.AddMtgJsonServices();

            return serviceCollection;
        }
    }
}