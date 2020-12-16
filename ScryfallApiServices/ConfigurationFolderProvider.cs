using System;
using System.IO;

namespace ScryfallApiServices
{
    public interface IConfigurationFolderProvider
    {
        string ApplicationName { get; }
        DirectoryInfo BaseFolder { get; }
    }

    public class ConfigurationFolderProvider : IConfigurationFolderProvider
    {
        public ConfigurationFolderProvider()
        {
            BaseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName));
        }

        public string ApplicationName => "ScryfallService";

        public DirectoryInfo BaseFolder { get; internal set; }
    }
}