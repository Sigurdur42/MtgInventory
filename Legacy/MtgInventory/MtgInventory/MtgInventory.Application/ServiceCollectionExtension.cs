using Microsoft.Extensions.DependencyInjection;
using MkmApi;
using MtgInventory.Service.Database;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Settings;
using ScryfallApiServices;
using TaskQueue;

namespace MtgInventory.Service
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMtgInventoryService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMkmApi();
            serviceCollection.AddScryfallApi();
            serviceCollection.AddGroupedScheduler();

            serviceCollection.AddSingleton<CardDatabase>();
            serviceCollection.AddSingleton<ITextDeckReader, TextDeckReader>();
            serviceCollection.AddSingleton<MtgInventoryService>();
            serviceCollection.AddSingleton<IAutoScryfallService, AutoScryfallService>();

            serviceCollection.AddSingleton<ISettingsService, SettingsService>();


            return serviceCollection;
        }
    }
}