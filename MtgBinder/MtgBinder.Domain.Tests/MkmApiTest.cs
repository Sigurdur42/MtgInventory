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
            var target = new MkmRequest();
            var result = target.GetArticles(new MkmAuthentication(), "290136", true);

            Assert.IsNotNull(result);
        }
    }
}
