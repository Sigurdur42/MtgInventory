namespace MtgBinders.Domain.Configuration
{
    public interface IJsonConfigurationSerializer
    {
        T Deserialize<T>(string targetFileName) where T : class;

        void Serialize<T>(string targetFileName, T objectToSerialize);
    }
}