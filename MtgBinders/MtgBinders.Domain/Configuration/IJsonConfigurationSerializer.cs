namespace MtgBinders.Domain.Configuration
{
    public interface IJsonConfigurationSerializer
    {
        T Deserialize<T>(string targetFileName, T defaultValue) where T : class;

        void Serialize<T>(string targetFileName, T objectToSerialize);
    }
}