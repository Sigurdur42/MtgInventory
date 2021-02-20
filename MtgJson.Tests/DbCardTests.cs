using MtgJson.Sqlite.Models;
using NUnit.Framework;

namespace MtgJson.Tests
{
    [TestFixture]
    public class DbCardTests
    {
       

        [TestCase("", "https://api.scryfall.com/cards/5da4136b-ad5b-4234-a476-90e1dcae00c6?format=image&face=front", TestName = "ScryfallImage-Face-Front")]
        [TestCase("a", "https://api.scryfall.com/cards/5da4136b-ad5b-4234-a476-90e1dcae00c6?format=image&face=front", TestName = "ScryfallImage-Face-Front")]
        [TestCase("b", "https://api.scryfall.com/cards/5da4136b-ad5b-4234-a476-90e1dcae00c6?format=image&face=back", TestName = "ScryfallImage-Face-Back")]
        public void VerifyScryfallImagePathFront(
            string side,
            string expected)
        {
            var target = new DbCard
            {
                ScryfallId = "5da4136b-ad5b-4234-a476-90e1dcae00c6",
                Side = side,
            };

            var result = target.GetScryfallImageUrl();
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}