using MtgBinders.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.Dropbox
{
    internal class LocalDropbox
    {
        public string FindDropboxFolder(
            IJsonConfigurationSerializer serializer)
        {
            var lookupFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            };

            foreach (var folder in lookupFolders)
            {
                var dropboxFile = Path.Combine(folder, "Dropbox", "info.json");

                var found = serializer.Deserialize<RootObject>(dropboxFile, null);
                if (found != null)
                {
                    return found.personal.path;
                }
            }

            return null;
        }
    }
}