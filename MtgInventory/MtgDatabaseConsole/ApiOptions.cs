using System.Collections.Generic;
using CommandLine;

namespace ScryfallApiConsole
{
    [Verb("Mirror", HelpText = "Generates a local mirror of the Scryfall database")]
    public class ApiOptions
    {
        [Option(shortName: 't', longName: "TargetFile", Required = false, HelpText = "Path to file where all information about used third party libraries is written to.")]
        public string TargetFile { get; set; } = "";

        [Option(shortName: 'c', longName: "Clear", Required = false, HelpText = "Clear the database first")]

        public bool Clear { get; set; }
    }
}