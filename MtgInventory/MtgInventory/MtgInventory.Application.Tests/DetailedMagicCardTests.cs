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
            // TODO: SpecFlow Test?
            var target = new DetailedMagicCard();
            target.UpdateFromTypeLine(typeLine);
            Assert.AreEqual(isBasicLand, target.IsBasicLand, nameof(target.IsBasicLand));
            Assert.AreEqual(isCreature, target.IsCreature, nameof(target.IsCreature));
            Assert.AreEqual(isArtifact, target.IsArtifact, nameof(target.IsArtifact));
        }
    }
}