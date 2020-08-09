using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReactiveUI;
using static System.Environment;

namespace MkmApi.TestUI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
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

        private string _output = "Initial value";
        public string Output
        {
            get => _output;

            set => this.RaiseAndSetIfChanged(ref _output, value);
        }

        public void OnDownloadSetsCommand()
        {

            Console.Write("");

            // Output = "Download sets pressed";
        }

        public void OnDownloadStock()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest();
            var result = request.GetStockAsCsv(AuthenticationData);

            stopwatch.Stop();

            var dump = $"Reading {result.Count()} took {stopwatch.Elapsed}";
            DisplayResult(dump);

            // Output = "Download sets pressed";

        }

        private void DisplayResult(string result)
        {
            Output = result;
        }

    }
}
