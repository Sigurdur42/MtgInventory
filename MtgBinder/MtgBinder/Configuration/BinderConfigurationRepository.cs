using System.IO;
using System.Text;

namespace MtgBinder.Configuration
{
    public interface IBinderConfigurationRepository
    {
        BinderConfiguration ReadConfiguration();

        void WriteConfiguration(BinderConfiguration configuration);
    }

    public class BinderConfigurationRepository : IBinderConfigurationRepository
    {
        private readonly UserDataFolderProvider _folderProvider;

        public BinderConfigurationRepository(UserDataFolderProvider folderProvider)
        {
            _folderProvider = folderProvider;
        }

        private FileInfo GetConfigurationFileName()
            => new FileInfo(Path.Combine(_folderProvider.ConfigurationFolder.FullName, "Config.yaml"));

        public BinderConfiguration ReadConfiguration()
        {
            var file = GetConfigurationFileName();
            if (!file.Exists)
            {
                var result = new BinderConfiguration();
                WriteConfiguration(result);
                return result;
            }

            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return deserializer.Deserialize<BinderConfiguration>(File.ReadAllText(file.FullName));
        }

        public void WriteConfiguration(BinderConfiguration configuration)
        {
            var file = GetConfigurationFileName();

            var serializer = new YamlDotNet.Serialization.Serializer();
            File.WriteAllText(file.FullName, serializer.Serialize(configuration), Encoding.UTF8);
        }
    }
}