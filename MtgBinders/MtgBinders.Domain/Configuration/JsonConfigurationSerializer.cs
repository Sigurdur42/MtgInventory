using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MtgBinders.Domain.Configuration
{
    internal class JsonConfigurationSerializer : IJsonConfigurationSerializer
    {
        public T Deserialize<T>(string targetFileName) where T : class
        {
            if (!File.Exists(targetFileName))
            {
                return null;
            }

            var content = File.ReadAllText(targetFileName);
            return JsonConvert.DeserializeObject<T>(content);
        }

        public void Serialize<T>(string targetFileName, T objectToSerialize)
        {
            var info = new FileInfo(targetFileName);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }

            var content = JsonConvert.SerializeObject(objectToSerialize);
            File.WriteAllText(targetFileName, content, Encoding.UTF8);
        }
    }
}