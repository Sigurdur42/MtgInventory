using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Environment;

namespace MkmApi.TestUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            GenerateMkmTokenFile();
        }

        private void GenerateMkmTokenFile()
        {
            var targetFile = new FileInfo(Path.Combine(
                Environment.GetFolderPath(SpecialFolder.MyDocuments),
                ".mkmAuthenticationData"));

            var reader = new AuthenticationReader();
            if (!targetFile.Exists)
            {
                // Create a dummy authentication file as template
                var target = new MkmAuthenticationData
                {
                    AppToken = "Insert AppToken",
                    AppSecret = "Insert AppSecret",
                    AccessSecret = "Insert AccessSecret",
                    AccessToken = "Insert AccessToken"
                };

                reader.WriteToYaml(targetFile, target);
            }

            AuthenticationData = reader.ReadFromYaml(targetFile);

        }

        public MkmAuthenticationData AuthenticationData { get; set; }
        public string Greeting => "Hello World!";
    }
}
