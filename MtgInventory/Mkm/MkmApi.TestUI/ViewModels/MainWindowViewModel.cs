using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Environment;

namespace MkmApi.TestUI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string _output = "Initial value";

        public MainWindowViewModel()
        {
            GenerateMkmTokenFile();
        }

        public MkmAuthenticationData AuthenticationData { get; set; }

        public string Output
        {
            get => _output;

            set => this.RaiseAndSetIfChanged(ref _output, value);
        }

        public void OnDownloadAllCards()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest();

            var index = 0;
            request.GetProductsAsCsv(AuthenticationData, (p) =>
            {
                ++index;
                Console.WriteLine($"have read product line: {p.Name}");
            });

            stopwatch.Stop();

            var dump = $"Reading {index} took {stopwatch.Elapsed}";
            DisplayResult(dump);

            // Output = "Download sets pressed";
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

        public void OnDownloadWithParameters()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest();
            var result = request.GetArticles(
                AuthenticationData,
                "16366",
                true,
                null);

            stopwatch.Stop();

            //var dump = $"Reading {result.Count()} took {stopwatch.Elapsed}";
            //DisplayResult(dump);

            // Output = "Download sets pressed";
        }

        private void DisplayResult(string result)
        {
            Output = result;
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
    }
}