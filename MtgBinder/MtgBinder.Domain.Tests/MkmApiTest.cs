using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace MtgBinder.Domain.Tests
{
    [TestFixture]
    public class MkmApiTest
    {
        [Test]
        [Explicit]
        public void TestMkmApi()
        {
            var target = new RequestHelper();
            var result = target.MakeRequest();

            Assert.IsNotNull(result);
        }
    }
}
