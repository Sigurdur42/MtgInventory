using Microsoft.Extensions.DependencyInjection;

namespace MtgJson
{
    public static class MtgJsonServiceServiceCollectionExtension
    {
        public static IServiceCollection AddMtgJsonServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMtgJsonService, MtgJsonService>();

            return serviceCollection;
        }
    }
}