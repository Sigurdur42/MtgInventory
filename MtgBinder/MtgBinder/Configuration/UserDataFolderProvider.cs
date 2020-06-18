using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MtgBinder.Configuration
{
    public class UserDataFolderProvider
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
