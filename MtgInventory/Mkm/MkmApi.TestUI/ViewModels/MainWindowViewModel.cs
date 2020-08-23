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
            var request = new MkmRequest(AuthenticationData);

            var index = 0;
            using (var products = request.GetProductsAsCsv())
            {
                foreach (var p in products.Products)
                {
                    ++index;
                    Console.WriteLine($"have read product line: {p.Name}");
                }
            };

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
            var request = new MkmRequest(AuthenticationData);
            var result = request.GetStockAsCsv();

            stopwatch.Stop();

            var dump = $"Reading {result.Count()} took {stopwatch.Elapsed}";
            DisplayResult(dump);

            // Output = "Download sets pressed";
        }

        public void OnDownloadSingleProduct()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest(AuthenticationData);
            var result = request.GetProductData("16366");

            stopwatch.Stop();

            var dump = $"Reading {result.NameEn} took {stopwatch.Elapsed}";
            DisplayResult(dump);

            // Output = "Download sets pressed";
        }

        public void OnDownloadGames()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest(AuthenticationData);
            var result = request.GetGames().OrderBy(g => g.IdGame);

            stopwatch.Stop();

            var all = string.Join(Environment.NewLine, result.Select(g => $"{g.IdGame} {g.Name}"));
            var dump = $"Reading {all} took {stopwatch.Elapsed}";
            DisplayResult(dump);

            // Output = "Download sets pressed";
        }

        public void OnDownloadWithParameters()
        {
            var stopwatch = Stopwatch.StartNew();
            var request = new MkmRequest(AuthenticationData);
            var result = request.GetArticles(
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