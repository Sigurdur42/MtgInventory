using System;
using Microsoft.Extensions.DependencyInjection;

namespace MtgJson.Sqlite
{
    public static class MtgJsonSqliteServiceServiceCollectionExtension
    {
        public static IServiceCollection AddMtgJsonSqliteServices(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddSingleton<IMtgJsonService, MtgJsonService>();
            //serviceCollection.AddSingleton<ILiteDbService, LiteDbService>();

            serviceCollection.AddMtgJsonServices();

            serviceCollection.AddSingleton<IMtgJsonMirrorIntoSqliteService, MtgJsonMirrorIntoSqliteService>();

            return serviceCollection;
        }
    }
}
