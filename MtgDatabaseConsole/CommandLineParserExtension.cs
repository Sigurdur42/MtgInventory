using System;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace ScryfallApiConsole
{
    public static class CommandLineParserExtension
    {
        public static ParserResult<T> ThrowOnParseError<T>(this ParserResult<T> result)
        {
            if (!(result is NotParsed<T>))
            {
                // Case with no errors needs to be detected explicitly, otherwise the .Select line will throw an InvalidCastException
                return result;
            }

            var builder = SentenceBuilder.Create();
            var errorMessages = (HelpText.RenderParsingErrorsTextAsLines(
                result, 
                builder.FormatError, 
                builder.FormatMutuallyExclusiveSetErrors, 
                1) ?? new string[0]).ToArray();

            if (!errorMessages.Any())
            {
                return result;
            }

            var dump = new StringBuilder();
            dump.AppendLine($"Parsing command line arguments failed:");
            dump.AppendLine(string.Join(Environment.NewLine, errorMessages.Select(l => $"- {l}")));

            dump.AppendLine();
            dump.AppendLine($"These are the possible command line arguments:");
            dump.AppendLine(HelpText.AutoBuild(result));

            throw new InvalidOperationException(dump.ToString());
        }
    }
}