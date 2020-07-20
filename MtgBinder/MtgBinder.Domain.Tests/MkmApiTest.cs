using System;
using System.Collections.Generic;
using System.Text;
using MtgBinder.Domain.Mkm;
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
            var result = target.MakeRequest(new MkmAuthentication());

            Assert.IsNotNull(result);
        }
    }
}
