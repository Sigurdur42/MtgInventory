using Microsoft.Extensions.DependencyInjection;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Settings;

namespace MtgInventory.Service
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMtgInventoryService(this IServiceCollection serviceCollection)
        {
            // TODO: Scryfall only manuall?
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();

            serviceCollection.AddSingleton<CardDatabase>();
            serviceCollection.AddSingleton<ITextDeckReader, TextDeckReader>();
            serviceCollection.AddSingleton<MtgInventoryService>();
            serviceCollection.AddSingleton<IAutoScryfallService, AutoScryfallService>();

            serviceCollection.AddSingleton<ISettingsService, SettingsService>();


            return serviceCollection;
        }
    }
}