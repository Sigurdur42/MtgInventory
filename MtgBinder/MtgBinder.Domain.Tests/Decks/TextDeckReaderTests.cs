using System.Linq;
using System.Reflection;
using MtgBinder.Domain.Decks;
using NUnit.Framework;

namespace MtgBinder.Domain.Tests.Decks
{
    [TestFixture]
    public class TextDeckReaderTests
    {
        [TestCase("SampleCommanderDeck.txt", "Kydele, Chosen of Kruphix", 1)]
        public void VerifyLoadedCardsDeckData(
            string embeddedResourceFile,
            string cardToLookFor,
            int numberOfExpectedCards)
        {
            var result = InternalRead(embeddedResourceFile).Deck;
            // find the wanted card
            var foundCard = result.Mainboard.FirstOrDefault(c => c.Name == cardToLookFor);
            var count = foundCard?.Count ?? 0;

            Assert.AreEqual(numberOfExpectedCards, count, "card count");
        }

        [TestCase("SampleCommanderDeck.txt", 100)]
        public void VerifyTotalCardCount(
            string embeddedResourceFile,
            int numberOfExpectedCards)
        {
            var result = InternalRead(embeddedResourceFile).Deck;

            Assert.AreEqual(numberOfExpectedCards, result.TotalCards, "card count");
        }

        private DeckReaderResult InternalRead(string embeddedResourceFile)
        {
            var resourceLoader = new ResourceLoader();
            var deckContent = resourceLoader.GetEmbeddedResourceString(GetType().Assembly, embeddedResourceFile);

            var deckName = "dummy";
            var target = new TextDeckReader();
            var result = target.ReadDeck(deckContent, deckName);

            Assert.AreEqual(deckName, result.Deck.Name, nameof(result.Deck.Name));
            return result;
        }
    }
}