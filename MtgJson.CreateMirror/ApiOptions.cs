using System.IO;
using CommandLine;

namespace MtgJson.CreateMirror
{
    [Verb("Mirror", HelpText = "Generates a local mirror of the MtgJson database")]
    public class ApiOptions
    {
        [Option('t', "targetFile", Required = true, HelpText = "Specify the target file for the SQLite database.")]
        public FileInfo TargetFile { get; set; } = new FileInfo(@"c:\temp\localMirror.sqlite");
    }
}