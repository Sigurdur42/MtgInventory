using System.IO;
using MtgDatabase.Decks;
using NUnit.Framework;

namespace MtgDatabase.Tests.Decks
{
    [TestFixture]
    public class TextDeckReaderTests
    {
        private string _testDataFolder = "";

        [OneTimeSetUp]
        public void Init()
        {
            _testDataFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "Decks", "TestData");
        }

        [Test]
        public void VerifyGaviCycleDeck()
        {
            var inputFile = Path.Combine(_testDataFolder, "Gavi Cycle.txt");
            Assert.That(File.Exists(inputFile), $"Cannot find input file {inputFile}");

            var reader = new TextDeckReader();
            var result = reader.ReadDeck(File.ReadAllText(inputFile), inputFile);
            Assert.That(result != null);

            Assert.AreEqual(100, result.Deck.GetTotalCardCount());
        }
    }
}