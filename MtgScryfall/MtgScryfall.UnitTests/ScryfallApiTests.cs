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
            var target = new ScryfallApi();
            var result = target.GetAllSets();
        }
    }
}