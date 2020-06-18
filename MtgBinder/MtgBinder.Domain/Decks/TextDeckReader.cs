using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MtgDomain;
using Serilog;

namespace MtgBinder.Domain.Decks
{
    public class TextDeckReader
    {
        public DeckReaderResult ReadDeck(
            string deckContent,
            string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckContent))
            {
                throw new InvalidOperationException("cannot read deck content from empty data.");
            }

            var result = new DeckList()
            {
                Name = deckName,
                Format = Format.Other,
            };

            var errorLines = new List<string>();

            var lines = deckContent
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var lineResult = AnalyseLine(line);
                if (lineResult == null)
                {
                    errorLines.Add(line);

                }
                else
                {
                    result.Mainboard.Add(lineResult);
                }
            }



            return new DeckReaderResult
            {
                Deck=result,
                UnreadLines = errorLines.ToArray(),
            };
        }

        private DeckItem AnalyseLine(
            string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var regexExpression = @"^\s*(?<count>\d+)[\sx\*]+(?<name>[^$]+)";
            var match = Regex.Match(line, regexExpression, RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                Log.Warning($"{nameof(TextDeckReader)}: Cannot read line '{line}'");
                return null;
            }

            var result = new DeckItem()
            {
                Name = match.Groups["name"].Value,
            };

            if (int.TryParse(match.Groups["count"].Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture,
                out var count))
            {
                result.Count = count;
            }

            return result;
        }
    }
}