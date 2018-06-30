using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.Scryfall;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.Services.Images;
using MtgScryfall;

namespace MtgBinders.Domain.DependencyInjection
{
    public static class BindMicrosoftDi
    {
        public static void BindProductiveEnvironment(
            ILogger logger,
            ServiceCollection serviceCollection)
        {
            logger.LogDebug($"Configuring DI for MtgBinders.Domain...");

            // Url: https://asp.net-hacker.rocks/2017/02/08/using-dependency-injection-in-dotnet-core-console-apps.html

            serviceCollection.AddSingleton<IBinderDomainConfigurationProvider, BinderDomainConfigurationProvider>();
            serviceCollection.AddSingleton<IScryfallApi, ScryfallApi>();
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();
            serviceCollection.AddSingleton<IMtgSetService, MtgSetService>();
            serviceCollection.AddSingleton<IMtgDatabaseService, MtgDatabaseService>();
            serviceCollection.AddSingleton<IMtgSetRepository, MtgSetRepository>();
            serviceCollection.AddSingleton<IMtgCardRepository, MtgCardRepository>();
            serviceCollection.AddSingleton<IJsonConfigurationSerializer, JsonConfigurationSerializer>();
            serviceCollection.AddSingleton<ICardSearchService, CardSearchService>();
            serviceCollection.AddSingleton<IMtgCardService, MtgCardService>();
            serviceCollection.AddSingleton<IMtgImageCache, MtgImageCache>();
            serviceCollection.AddSingleton<IMtgWantsListService, MtgWantsListService>();

            //// serviceCollection.AddSingleton<IScryfallService>((serviceProvider) => new ScryfallService(serviceProvider.GetService<IScryfallApi>()));
        }
    }
}