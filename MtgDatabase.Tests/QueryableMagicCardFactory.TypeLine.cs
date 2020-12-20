using NUnit.Framework;

namespace MtgDatabase.Tests
{
    [TestFixture]
    public partial class QueryableMagicCardFactoryTests
    {
        [TestCase(@"Plane — Alara", false)]
        [TestCase(@"Legendary Snow Enchantment", true)]
        [TestCase(@"Legendary Snow Land", true)]
        [TestCase(@"Basic Snow Land — Forest", true)]
        [TestCase(@"Snow Artifact Creature — Construct", true)]
        public void VerifyTypeLineIsSnow(string typeLine, bool isSnow)
        {
            _factory?.UpdateFromTypeLine(_card!, typeLine);
            Assert.AreEqual(isSnow, _card!.IsSnow);
        }
    }
}