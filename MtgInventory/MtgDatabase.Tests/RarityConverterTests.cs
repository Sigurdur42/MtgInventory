using MtgDatabase.Models;
using NUnit.Framework;

namespace MtgDatabase.Tests
{
    [TestFixture]
    public class RarityConverterTests
    {
        [TestCase("common", null, Rarity.Common)]
        [TestCase("common", "Basic Land — Swamp", Rarity.BasicLand)]
        [TestCase("common", "Basic Snow Land — Forest", Rarity.BasicLand)]   
        public void VerifyRarityConversion(string rarity, string typeLine, Rarity expected)
        {
            var result = rarity.ToRarity(typeLine);
            Assert.AreEqual(expected, result);
        }
    }
}