using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace MtgDatabase.Decks
{
    public enum DeckLineType
    {
        Card = 0,
        Category = 1,
        Comment = 2,
    }

    public interface ITextDeckReader
    {
        DeckReaderResult ReadDeck(
            string deckContent,
            string deckName);
    }

    public class TextDeckReader : ITextDeckReader
    {
        private readonly ILogger<TextDeckReader> _logger;

        public TextDeckReader(ILogger<TextDeckReader> logger)
        {
            _logger = logger;
        }

        public DeckReaderResult ReadDeck(
            string deckContent,
            string deckName)
        {
            if (string.IsNullOrWhiteSpace(deckContent))
            {
                _logger.LogError($"Deck content is empty (Name: {deckName}");
                throw new InvalidOperationException("cannot read deck content from empty data.");
            }

            var result = new Deck()
            {
                Name = deckName,
                // Format = Format.Other,
            };

            var errorLines = new List<string>();

            var lines = deckContent
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var currentCategory = new DeckCategory()
            {
                CategoryName = "Mainboard",
            };

            result.Categories.Add(currentCategory);

            // TODO: Remove empty categories later

            foreach (var line in lines)
            {
                var lineResult = AnalyseLine(line);
                switch (lineResult?.LineType)
                {
                    case null:
                        errorLines.Add(line);
                        break;

                    case DeckLineType.Card:
                        currentCategory.Lines.Add(lineResult);
                        break;

                    case DeckLineType.Category:
                        currentCategory = new DeckCategory()
                        {
                            CategoryName = lineResult.CardName,
                        };
                        break;

                    case DeckLineType.Comment:
                        currentCategory.Lines.Add(lineResult);
                        break;
                }
            }

            return new DeckReaderResult
            {
                Deck = result,
                UnreadLines = errorLines.ToArray(),
            };
        }

        private DeckLine? AnalyseLine(
            string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            // TODO: Check for comments
            var regexExpression = @"^\s*(?<count>\d+)[\sx\*]+(?<name>[^$]+)";
            var match = Regex.Match(line, regexExpression, RegexOptions.CultureInvariant);

            if (!match.Success)
            {
                _logger.LogTrace($"Cannot read line '{line}'");
                return new DeckLine();
            }

            var result = new DeckLine()
            {
                CardName = match.Groups["name"].Value,
            };

            if (int.TryParse(match.Groups["count"].Value ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture,
                out var count))
            {
                result.Quantity = count;
            }

            // TODO: line type

            result.OriginalLine = line;
            return result;
        }


    }
}