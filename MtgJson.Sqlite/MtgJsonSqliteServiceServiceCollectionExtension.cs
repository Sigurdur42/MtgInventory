using System;
using Microsoft.Extensions.DependencyInjection;

namespace MtgJson.Sqlite
{
    public static class MtgJsonSqliteServiceServiceCollectionExtension
    {
        public static IServiceCollection AddMtgJsonServices(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddSingleton<IMtgJsonService, MtgJsonService>();
            //serviceCollection.AddSingleton<ILiteDbService, LiteDbService>();

            serviceCollection.AddMtgJsonServices();



            return serviceCollection;
        }
    }
}
