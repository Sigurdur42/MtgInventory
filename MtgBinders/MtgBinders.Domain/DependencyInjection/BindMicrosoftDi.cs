using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Scryfall;
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

            serviceCollection.AddSingleton<IScryfallApi, ScryfallApi>();
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();
            // serviceCollection.AddSingleton<IScryfallService>((serviceProvider) => new ScryfallService(serviceProvider.GetService<IScryfallApi>()));
        }
    }
}