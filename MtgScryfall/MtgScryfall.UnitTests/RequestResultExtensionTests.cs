using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MtgScryfall.UnitTests
{
    [TestFixture]
    class RequestResultExtensionTests
    {
        [Test]
        public void MonetaryValueDeserializeTest()
        {
            var request = new RequestResult
            {
                StatusCode = 200,
                Success = true,
                JsonResult = TestData.MoneyTestDeserializeData
            };

            var result = request.DeserializeCardData();
            Assert.IsNotNull(result.CardData[0].PriceUsd, "PriceUsd");
            // Assert.IsNotNull(result.CardData[0].PriceEur, "PriceEur");
        }
    }
}
