using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MtgInventory.Service.ReferenceData
{
    public class CardReferenceDataReader
    {
        public CardReferenceData[] ReadEmbedded()
        {
            var resourceLoader = new ResourceLoader();
            var yaml = resourceLoader.GetEmbeddedResourceString(
                GetType().Assembly,
                "ReferenceCardData.yaml");

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<CardReferenceData[]>(yaml);
        }

        public void Write(FileInfo targetFile, CardReferenceData[] data)
        {
            if (!targetFile.Directory?.Exists ?? false)
            {
                targetFile.Directory?.Create();
            }

            var serializer = new SerializerBuilder().Build();

            var yaml = serializer.Serialize(data);
            File.WriteAllText(targetFile.FullName, yaml);
        }
    }
}