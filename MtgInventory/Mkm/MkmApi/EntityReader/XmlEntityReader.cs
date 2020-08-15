using MkmApi.Entities;
using System.Xml.Linq;

namespace MkmApi.EntityReader
{
    internal static class XmlEntityReader
    {
        public static Article ReadArticle(this XElement articleElement)
        {
            var result = new Article
            {
                IdArticle = articleElement.GetContent("idArticle"),
                IdProduct = articleElement.GetContent("idProduct"),
                Language = articleElement.Element("language").ReadLanguage(),
                Comments = articleElement.GetContent("comments"),
                Price = articleElement.GetContent("price"),
                PriceEur = articleElement.GetContent("priceEUR"),
                PriceGbp = articleElement.GetContent("priceGBP"),
                Count = articleElement.GetContent("count"),
                InShoppingCart = articleElement.GetContent("inShoppingCart"),
                Seller = articleElement.Element("seller").ReadUser(),
                Condition = articleElement.GetContent("condition"),
                IsFoil = articleElement.GetContent("isFoil"),
                IsSigned = articleElement.GetContent("isSigned"),
                IsPlayset = articleElement.GetContent("isPlayset")
                // TODO: links
            };

            return result;
        }

        public static Language ReadLanguage(this XElement element)
        {
            return new Language
            {
                Id = element.GetContent("idLanguage"),
                Name = element.GetContent("languageName")
            };
        }

        public static User ReadUser(this XElement element)
        {
            return new User
            {
                IdUser = element.GetContent("idUser"),
                UserName = element.GetContent("username"),
                RegistrationDate = element.GetContent("registrationDate"),
                IsCommercial = element.GetContentInt("isCommercial"),
                IsSeller = element.GetContent("isSeller"),

                // todo: name

                // todo: address

                Phone = element.GetContent("phone"),
                Email = element.GetContent("email"),
                Vat = element.GetContent("vat"),
                LegalInformation = element.GetContent("legalInformation"),
                RiskGroup = element.GetContentInt("riskGroup"),
                LossPercentage = element.GetContent("lossPercentage"),
                UnsentShipments = element.GetContent("unsentShipments"),
                Reputation = element.GetContentInt("reputation"),
                ShipsFast = element.GetContent("shipsFast"),
                SellCount = element.GetContent("sellCount"),
                SoldItems = element.GetContentInt("soldItems"),
                AvgShippingTime = element.GetContent("avgShippingTime"),
                OnVacation=element.GetContent("onVacation")
            };
        }

        public static string GetContent(this XElement element, string name)
        {
            return element?.Element(name)?.Value;
        }

        public static int GetContentInt(this XElement element, string name)
        {
            string content = element?.Element(name)?.Value;
            return content.ToInt();
        }
    }
}