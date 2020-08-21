using System;
using System.IO;

namespace MtgInventory.Service
{
    public class SystemFolders
    {
        public SystemFolders()
        {
            BaseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName));
        }

        public string ApplicationName => "MtgInventory";

        public DirectoryInfo BaseFolder { get; internal set; }
    }
}