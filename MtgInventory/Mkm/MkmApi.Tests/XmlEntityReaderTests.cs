using MkmApi.EntityReader;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace MkmApi.Tests
{
    [TestFixture]
    public class XmlEntityReaderTests
    {
        private XDocument _articles16366;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var resourceLoader = new ResourceLoader();
            _articles16366 = XDocument.Parse(resourceLoader.GetEmbeddedResourceString(this.GetType().Assembly, "Articles_16366.txt"));
        }

        [Test]
        public void VerifyReadArticles16366()
        {
            var articles = _articles16366.Root.Elements("article").Select(a => a.ReadArticle()).ToArray();

            Assert.AreEqual(10, articles.Length, "expected 10 articles here");
        }
    }
}