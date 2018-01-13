using NUnit.Framework;
using System;

namespace MtgScryfall.UnitTests
{
    [TestFixture]
    public class ScryfallApiTests
    {
        [Test]
        public void ScryfallApi_GetAllSet_Test()
        {
            var target = new ScryfallApi();
            var result = target.GetAllSets();
        }
    }
}
