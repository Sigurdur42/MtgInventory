using Castle.Core.Logging;
using Moq;
using NUnit.Framework;

namespace MtgScryfall.UnitTests
{
    [TestFixture]
    public class ScryfallApiTests
    {
        [Test]
        public void ScryfallApi_DeserializeSetData_Test()
        {
            var target = new RequestResult()
            {
                Success = true,
                JsonResult = TestData.SetDataJson
            };

            target.DeserializeSetData();
        }

        [Test]
        public void ScryfallApi_GetAllSet_Test()
        {
            var target = new ScryfallApi(null);
            var result = target.GetAllSets();
        }

        [Test]
        public void ScryfallApi_GetCardsBySet_Test()
        {
            var target = new ScryfallApi(null);
            var result = target.GetCardsBySet("AER");
        }

        [Test]
        public void ScryfallApi_GetCardById_Test()
        {
            var target = new ScryfallApi(null);
            var result = target.GetCardByScryfallId("1af0c9a0-0dfa-4245-8b29-7bd37982b7d2");
        }
    }
}