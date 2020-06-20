using System;
using System.Diagnostics;
using System.IO;

namespace MtgBinder.Domain.Configuration
{
    public class UserDataFolderProvider : IUserDataFolderProvider
    {
        public UserDataFolderProvider()
        {
            var subFolderName = "MtgBinder";
            if (Debugger.IsAttached)
            {
                subFolderName += "_Debug";
            }

            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigurationFolder = new DirectoryInfo(Path.Combine(baseFolder, subFolderName));
            if (!ConfigurationFolder.Exists)
            {
                ConfigurationFolder.Create();
            }

        }
        public DirectoryInfo ConfigurationFolder { get; }
    }
}
