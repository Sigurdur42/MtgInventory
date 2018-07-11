using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MtgBinders.Domain.Configuration
{
    internal class JsonConfigurationSerializer : IJsonConfigurationSerializer
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
        };

        public T Deserialize<T>(string targetFileName) where T : class
        {
            if (!File.Exists(targetFileName))
            {
                return null;
            }

            var content = File.ReadAllText(targetFileName);
            return JsonConvert.DeserializeObject<T>(content, _settings);
        }

        public void Serialize<T>(string targetFileName, T objectToSerialize)
        {
            var info = new FileInfo(targetFileName);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }

            var content = JsonConvert.SerializeObject(objectToSerialize, _settings);
            File.WriteAllText(targetFileName, content, Encoding.UTF8);
        }
    }
}