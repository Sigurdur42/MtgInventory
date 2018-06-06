using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MtgBinders.Domain.Scryfall;
using MtgScryfall;

namespace MtgBinders.Domain.DependencyInjection
{
    public static class BindMicrosoftDi
    {
        public static void BindProductiveEnvironment(ServiceCollection serviceCollection)
        {
            // Url: https://asp.net-hacker.rocks/2017/02/08/using-dependency-injection-in-dotnet-core-console-apps.html

            serviceCollection.AddSingleton<IScryfallApi, ScryfallApi>();
            serviceCollection.AddSingleton<IScryfallService, ScryfallService>();
            // serviceCollection.AddSingleton<IScryfallService>((serviceProvider) => new ScryfallService(serviceProvider.GetService<IScryfallApi>()));
        }
    }
}
