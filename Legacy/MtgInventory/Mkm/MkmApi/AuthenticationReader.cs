using System.IO;
namespace MkmApi
{
    public class AuthenticationReader
    {
        public MkmAuthenticationData ReadFromYaml(FileInfo fileToLoad)
        {
            if (!fileToLoad.Exists)
            {
                return new MkmAuthenticationData();
            }

            var content = File.ReadAllText(fileToLoad.FullName);
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            
            return deserializer.Deserialize<MkmAuthenticationData>(content);
        }

        public void WriteToYaml(FileInfo targetFile, MkmAuthenticationData data)
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            var content = serializer.Serialize(data);
            File.WriteAllText(targetFile.FullName, content);
        }
    }
}