using Microsoft.Extensions.DependencyInjection;

namespace MtgBinder.Wpf
{
    internal static class ApplicationSingeltons
    {
        public static ServiceProvider ServiceProvider
        {
            get; set;
        }
    }
}