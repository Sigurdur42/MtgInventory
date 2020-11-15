using MtgInventory.Service.Models;
using NUnit.Framework;

namespace MtgInventory.Application.Tests
{
    [TestFixture]
    public class DetailedMagicCardTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Creature — Human Advisor ", false, true, false)]
        [TestCase("Basic Land — Island ", true, false, false)]
        [TestCase("Artifact Creature — Construct ", false, true, true)]
        public void VerifyUpdateFromTypeLine(
            string typeLine,
            bool isBasicLand,
            bool isCreature,
            bool isArtifact)
        {
            var target = new DetailedMagicCard();
            target.UpdateFromTypeLine(typeLine);
            Assert.AreEqual(isBasicLand, target.IsBasicLand, nameof(target.IsBasicLand));
            Assert.AreEqual(isCreature, target.IsCreature, nameof(target.IsCreature));
            Assert.AreEqual(isArtifact, target.IsArtifact, nameof(target.IsArtifact));
        }

        [TestCase("Amonkhet Punch Card", true, false, false, false)]
        [TestCase("Gideon of the Trials Emblem", false, true, false, false)]
        [TestCase("Energy reserve", false, false, true, false)]
        [TestCase("The Monarch", false, false, true, false)]
        [TestCase("Experience Counter", false, false, true, false)]
        [TestCase("Poison Counter", false, false, true, false)]
        [TestCase("Tip: Decks", false, false, false, true)]
        [TestCase("Rules Tip: Legends Cards", false, false, false, true)]
        [TestCase("Arena Code (Kaleidoscope Killers)", false, false, false, false, true)]
        [TestCase("Magic Online Code (Secret Lair Drop Bundle)", false, false, false, false, true)]
        public void VerifyUpdateFromName(
            string name,
            bool isPunchCard = false,
            bool isEmblem = false,
            bool isToken = false,
            bool isTipCard = false,
            bool isOnlineCode = false)
        {
            var target = new DetailedMagicCard();
            target.UpdateFromName(name);
            Assert.AreEqual(isPunchCard, target.IsPunchCard, nameof(target.IsPunchCard));
            Assert.AreEqual(isEmblem, target.IsEmblem, nameof(target.IsEmblem));
            Assert.AreEqual(isToken, target.IsToken, nameof(target.IsToken));
            Assert.AreEqual(isTipCard, target.IsTipCard, nameof(target.IsTipCard));
            Assert.AreEqual(isOnlineCode, target.IsOnlineCode, nameof(target.IsOnlineCode));
        }
    }
}