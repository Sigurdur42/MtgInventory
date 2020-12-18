using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LocalSettings
{
    public static class LocalSettingServiceExtension
    {
        public static T GetComplexValue<T>(this ILocalSettingService service, string key, T defaultValue)
        {
            var foundString = service.Get(key);
            if (foundString == null)
            {
                return defaultValue;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return deserializer.Deserialize<T>(foundString);
        }

        public static void SetComplexValue<T>(this ILocalSettingService service, string key, T value)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var content = serializer.Serialize(value);
            service.Set(key, content);
        }
    }
}