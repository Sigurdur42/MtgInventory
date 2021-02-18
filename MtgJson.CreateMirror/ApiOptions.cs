using System.IO;
using CommandLine;

namespace MtgJson.CreateMirror
{
    [Verb("Mirror", HelpText = "Generates a local mirror of the MtgJson database")]
    public class ApiOptions
    {
        [Option('t', "targetFile", Required = true, HelpText = "Specify the target file for the SQLite database.")]
        public FileInfo TargetFile { get; set; } = new FileInfo(@"c:\temp\localMirror.sqlite");

        [Option('p', "priceOnly", Required = false, HelpText = "Specify this to only download price data.")]
        public bool PriceOnly { get; set; }

        [Option('d', "DebugMode", Required = false, HelpText = "Debug mode - uses local files from your Downloads folder.")]
        public bool DebugMode { get; set; }
    }
}