﻿using System.Xml.Linq;
using MkmApi.Entities;

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
                OnVacation = element.GetContent("onVacation"),
            };
        }

        public static Game ReadGame(this XElement node)
        {
            return new Game
            {
                IdGame = node.GetContentInt("idGame"),
                Name = node.GetContent("name"),
                Abbreviation = node.GetContent("abbreviation")
            };
        }

        public static Expansion ReadExpansion(this XElement node)
        {
            var releaseDate = node.GetContent("releaseDate");

            return new Expansion
            {
                Icon = node.GetContent("icon"),
                ReleaseDate = releaseDate,
                IdExpansion = node.GetContentInt("idExpansion"),
                IdGame = node.GetContentInt("idGame"),
                EnName = node.GetContent("enName"),
                Abbreviation = node.GetContent("abbreviation"),
                IsReleased = node.GetContent("isReleased"),
            };
        }

        public static PriceGuide ReadPriceGuide(this XElement priceGuide)
        {
            return new PriceGuide
            {
                PriceSell = GetContentDecimal(priceGuide, "SELL"),
                PriceLow = GetContentDecimal(priceGuide, "LOW"),
                PriceLowEx = GetContentDecimal(priceGuide, "LOWEX"),
                PriceLowFoil = GetContentDecimal(priceGuide, "LOWFOIL"),
                PriceAverage = GetContentDecimal(priceGuide, "AVG"),
                PriceTrend = GetContentDecimal(priceGuide, "TREND"),
                PriceTrendFoil = GetContentDecimal(priceGuide, "TRENDFOIL"),
            };
        }

        public static Product ReadProduct(this XElement product)
        {
            return new Product()
            {
                IdProduct = product?.Element("idProduct")?.Value ?? "",
                IdMetaproduct = product?.Element("idMetaproduct")?.Value ?? "",
                NameEn = product?.Element("enName")?.Value ?? "",
                WebSite = product?.Element("website")?.Value ?? "",
                Image = product?.Element("image")?.Value ?? "",
                CountReprints = product?.GetContentInt("countReprints") ?? 0,

                PriceGuide = product?.Element("priceGuide")?.ReadPriceGuide() ?? new PriceGuide(),
                OriginalResponse = product?.ToString(SaveOptions.None) ?? "",
            };
        }

        public static string GetContent(this XElement element, string name)
        {
            return element?.Element(name)?.Value ?? "";
        }

        public static int GetContentInt(this XElement element, string name)
        {
            string content = element?.Element(name)?.Value ?? "";
            return content.ToInt();
        }

        public static decimal? GetContentDecimal(this XElement element, string name)
        {
            string content = element?.Element(name)?.Value ?? "";
            return content.ToDecimal();
        }
    }
}