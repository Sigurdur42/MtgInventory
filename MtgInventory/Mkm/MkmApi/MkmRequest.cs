using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using CsvHelper;
using Microsoft.Extensions.Logging;
using MkmApi.Entities;
using MkmApi.EntityReader;

namespace MkmApi
{
    public class MkmRequest : IMkmRequest
    {
        private const string _loggerKey = nameof(MkmRequest);
        private readonly IApiCallStatistic _apiCallStatistic;
        private readonly MkmGoodCiticenAutoSleep _autoSleep = new MkmGoodCiticenAutoSleep();
        private readonly List<QueryParameter> _emptyParameters = new List<QueryParameter>();
        private readonly ILogger _logger;

        public MkmRequest(
            IApiCallStatistic apiCallStatistic,
            ILoggerFactory loggerFactory)
        {
            _apiCallStatistic = apiCallStatistic;
            _logger = loggerFactory.CreateLogger(_loggerKey);
        }

        public IEnumerable<Product> FindProducts(
            MkmAuthenticationData authenticationData,
            string productName,
            bool searchExact)
        {
            var queryParameters = new List<QueryParameter>
            {
                new QueryParameter("search", productName),
                new QueryParameter("idGame", "1"), 
                new QueryParameter("idLanguage", "1"), 
                new QueryParameter("maxResults", "10000")
            };

            if (searchExact)
            {
                queryParameters.Add(new QueryParameter("exact", "true"));
            }

            try
            {
                var response = MakeRequest(
                    authenticationData,
                    $"products/find",
                    queryParameters);

                var doc = XDocument.Parse(response);
                return doc.Root
                    .Elements("product")
                    .Select(g => g.ReadProduct())
                    .ToArray();
            }
            catch (Exception error)
            {
                _logger.LogError($"Error in product in {nameof(FindProducts)}({productName}, {searchExact}): {error}");
            }

            return new List<Product>();
        }

        public IEnumerable<Article> GetArticles(
            MkmAuthenticationData authenticationData,
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
                authenticationData,
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

        public IEnumerable<Expansion> GetExpansions(MkmAuthenticationData authenticationData, int gameId)
        {
            var response = MakeRequest(
                authenticationData,
                $"games/{gameId}/expansions",
                _emptyParameters);

            var doc = XDocument.Parse(response);
            return doc.Root
                .Elements("expansion")
                .Select(g => g.ReadExpansion())
                .ToArray();
        }

        public IEnumerable<Game> GetGames(MkmAuthenticationData authenticationData)
        {
            var response = MakeRequest(
                authenticationData,
                $"games",
                _emptyParameters);

            var doc = XDocument.Parse(response);
            return doc.Root
                .Elements("game")
                .Select(g => g.ReadGame())
                .ToArray();
        }

        public Product GetProductData(MkmAuthenticationData authenticationData, string productId)
        {
            var response = MakeRequest(
                authenticationData,
                $"products/{productId}",
                _emptyParameters);

            var doc = XDocument.Parse(response);
            var product = doc.Root.Element("product");

            var result = product.ReadProduct();
            result.WebSite = "https://www.cardmarket.com" + result.WebSite;
            return result;
        }

        public ProductCsvData GetProductsAsCsv(MkmAuthenticationData authenticationData)
        {
            var response = MakeRequest(
                authenticationData,
                "productlist",
                _emptyParameters);

            return new ProductCsvData(response);
        }

        public IEnumerable<MkmStockItem> GetStockAsCsv(MkmAuthenticationData authenticationData)
        {
            var response = MakeRequest(
                authenticationData,
                "stock/file",
                _emptyParameters);

            var doc = XDocument.Parse(response);
            var stockNode = doc.Root.Element("stock");

            var bytes = Convert.FromBase64String(stockNode.Value);

            using var decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
            using var decompressedStreamReader = new StreamReader(decompressionStream, Encoding.UTF8);
            using var csv = new CsvReader(decompressedStreamReader, CultureInfo.InvariantCulture);

            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = ";";
            csv.Configuration.BadDataFound = (context) => { };
            return csv.GetRecords<MkmStockItem>().ToList();
        }

        internal string MakeRequest(
            MkmAuthenticationData authenticationData,
            string urlCommand,
            List<QueryParameter> parameters)
        {
            if (!authenticationData.IsValid())
            {
                var error = $"MKM authentication data is not set. Please configure MKM data before calling the API functions.";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            _autoSleep.AutoSleep();

            parameters = parameters?.OrderBy(p => p.Name)?.ToList() ?? new List<QueryParameter>();

            IncrementCallStatistic();

            var method = "GET";
            var url = $"https://api.cardmarket.com/ws/v2.0/{urlCommand}";

            var httpUrl = url + parameters?.GenerateQueryString();
            var request = WebRequest.CreateHttp(httpUrl);

            var stopwatch = Stopwatch.StartNew();
            _logger.LogTrace($"Calling {httpUrl}...");
            var header = new OAuthHeader(authenticationData);
            request.Headers.Add(HttpRequestHeader.Authorization, header.GetAuthorizationHeader(method, url, parameters));
            request.Method = method;

            var response = request.GetResponse() as HttpWebResponse;
            var encoding = response?.CharacterSet == "" ?
                Encoding.UTF8 :
                Encoding.GetEncoding(response?.CharacterSet ?? "");

            using var stream = response.GetResponseStream();

            var reader = new StreamReader(stream, encoding);
            var result = reader.ReadToEnd();
            stopwatch.Stop();
            _logger.LogTrace($"Call to {httpUrl} took {stopwatch.Elapsed} exited with {response.StatusCode}");
            return result;
        }

        private void IncrementCallStatistic()
        {
            if (_apiCallStatistic == null)
            {
                return;
            }

            var today = DateTime.Now.Date;
            if (_apiCallStatistic.Today.Date != today)
            {
                _apiCallStatistic.Today = today;
                _apiCallStatistic.CountToday = 0;
            }

            _apiCallStatistic.CountToday += 1;
            _apiCallStatistic.CountTotal += 1;
        }
    }
}