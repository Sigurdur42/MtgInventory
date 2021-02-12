﻿using Microsoft.Extensions.DependencyInjection;
using MtgDatabase.Cache;
using MtgDatabase.Decks;
using MtgDatabase.Scryfall;
using MtgJson;
using ScryfallApiServices;

namespace MtgDatabase
{
    public static class MtgDatabaseServiceCollectionExtension
    {
        public static IServiceCollection AddMtgDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScryfallApi();

            serviceCollection.AddSingleton<IMirrorScryfallDatabase, MirrorScryfallDatabase>();
            serviceCollection.AddSingleton<IMtgDatabaseService, MtgDatabaseService>();
            serviceCollection.AddSingleton<IImageCache, ImageCache>();
            serviceCollection.AddSingleton<Database.MtgDatabase>();
            serviceCollection.AddSingleton<IMkmMapper, MkmMapper>();
            serviceCollection.AddSingleton<IAutoAupdateMtgDatabaseService, AutoAupdateMtgDatabaseService>();

            serviceCollection.AddSingleton<ITextDeckReader, TextDeckReader>();

            serviceCollection.AddMtgJsonServices();

            return serviceCollection;
        }
    }
}