using CsvHelper;
using MkmApi.Entities;
using MkmApi.EntityReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace MkmApi
{
    public class MkmRequest
    {
        public IEnumerable<Article> GetArticles(
            MkmAuthenticationData authentication,
            string productId,
            bool commercialOnly,
            IEnumerable<QueryParameter> queryParameters)
        {
            var parameters = new List<QueryParameter>(queryParameters ?? new QueryParameter[0]);
            if (commercialOnly)
            {
                parameters.Add(new QueryParameter("userType", "private"));
                parameters.Add(new QueryParameter("idLanguage", "1"));
                parameters.Add(new QueryParameter("minCondition", "NM"));
                parameters.Add(new QueryParameter("start", "0"));
                parameters.Add(new QueryParameter("maxResults", "10"));
            }

            var response = MakeRequest(
                authentication,
                $"articles/{productId}", parameters);

            var doc = XDocument.Parse(response);
            var articles = doc.Root.Elements("article")
                .Select(a => a.ReadArticle())
                .ToArray();

            return articles;

            ////var priceGuide = product.Element("priceGuide");

            ////return new MkmProductMagic()
            ////{
            ////    ProductId = product?.Element("idProduct")?.Value,
            ////    Name = product?.Element("enName")?.Value,
            ////    WebSite = product?.Element("website")?.Value,

            ////    PriceSell = priceGuide?.Element("SELL")?.Value,
            ////    PriceLow = priceGuide?.Element("LOW")?.Value,
            ////    PriceLowEx = priceGuide?.Element("LOWEX")?.Value,
            ////    PriceLowFoil = priceGuide?.Element("LOWFOIL")?.Value,
            ////    PriceAverage = priceGuide?.Element("AVG")?.Value,
            ////    PriceTrend = priceGuide?.Element("TREND")?.Value,
            ////    PriceTrendFoil = priceGuide?.Element("TRENDFOIL")?.Value,
            ////};
        }

        public IEnumerable<Game> GetGames(MkmAuthenticationData authentication)
        {
            var response = MakeRequest(
                authentication,
                $"games",
                null);

            var doc = XDocument.Parse(response);
            return doc.Root
                .Elements("game")
                .Select(g => g.ReadGame())
                .ToArray();
        }

        public Product GetProductData(MkmAuthenticationData authentication, string productId)
        {
            var response = MakeRequest(
                authentication,
                $"products/{productId}",
                null);

            var doc = XDocument.Parse(response);
            var product = doc.Root.Element("product");

            return product.ReadProduct();
        }

        public void GetProductsAsCsv(
            MkmAuthenticationData authentication,
            Action<MkmProductInfo> readSingleProductCallback)
        {
            var response = MakeRequest(
                authentication,
                "productlist",
                null);

            var doc = XDocument.Parse(response);
            var node = doc.Root.Element("productsfile");

            var bytes = Convert.FromBase64String(node.Value);

            using (var decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                //// using (var fileStream = File.Create (Path.Combine ("/home/michael", "temp.txt"))) {
                ////     decompressionStream.CopyTo (fileStream);
                //// }

                using (var decompressedStreamReader = new StreamReader(decompressionStream, Encoding.UTF8))
                {
                    using (var csv = new CsvReader(decompressedStreamReader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.BadDataFound = (context) =>
                        {
                            // var debug = 0;
                        };

                        foreach (var product in csv.GetRecords<MkmProductInfo>())
                        {
                            readSingleProductCallback?.Invoke(product);
                        }
                    }
                }
            }
        }

        public IEnumerable<MkmStockItem> GetStockAsCsv(MkmAuthenticationData authentication)
        {
            var response = MakeRequest(
                authentication,
                "stock/file",
                null);

            var doc = XDocument.Parse(response);
            var stockNode = doc.Root.Element("stock");

            var bytes = Convert.FromBase64String(stockNode.Value);

            using (var decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                using (var decompressedStreamReader = new StreamReader(decompressionStream, Encoding.UTF8))
                {
                    using (var csv = new CsvReader(decompressedStreamReader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Delimiter = ";";
                        csv.Configuration.BadDataFound = (context) => { };
                        return csv.GetRecords<MkmStockItem>().ToList();
                    }
                }
            }
        }

        internal string MakeRequest(
            MkmAuthenticationData authentication,
            string urlCommand,
            List<QueryParameter> parameters)
        {
            parameters = parameters?.OrderBy(p => p.Name)?.ToList() ?? new List<QueryParameter>();

            var method = "GET";
            var url = $"https://api.cardmarket.com/ws/v2.0/{urlCommand}";

            var request = WebRequest.CreateHttp(url + parameters?.GenerateQueryString()) as HttpWebRequest;
            var header = new OAuthHeader(authentication);
            request.Headers.Add(HttpRequestHeader.Authorization, header.GetAuthorizationHeader(method, url, parameters));
            request.Method = method;

            var response = request.GetResponse() as HttpWebResponse;
            var encoding = response.CharacterSet == "" ?
                Encoding.UTF8 :
                Encoding.GetEncoding(response.CharacterSet);

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
        }
    }
}