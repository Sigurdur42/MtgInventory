namespace MtgBinders.Domain.Configuration
{
    public interface IBinderDomainConfigurationProvider
    {
        string AppDataFolder { get; }
        bool IsInitialized { get; }

        void Initialize(string appDataBasePath);
    }
}