using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using CsvHelper;

namespace MkmApi {
    public class MkmRequest {
        public IEnumerable<MkmStockItem> GetStockAsCsv (MkmAuthenticationData authentication) {
            var response = MakeRequest (
                authentication,
                "stock/file",
                "");

            var doc = XDocument.Parse (response);
            var stockNode = doc.Root.Element ("stock");

            var bytes = Convert.FromBase64String (stockNode.Value);

            using (var decompressionStream = new GZipStream (new MemoryStream (bytes), CompressionMode.Decompress)) {
                using (var decompressedStreamReader = new StreamReader (decompressionStream, Encoding.UTF8)) {
                    using (var csv = new CsvReader (decompressedStreamReader, CultureInfo.InvariantCulture)) {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Delimiter = ";";
                        csv.Configuration.BadDataFound = (context) => {
                            // var debug = 0;
                        };
                        return csv.GetRecords<MkmStockItem> ().ToList ();
                    }
                }
            }
        }

        public void GetProductsAsCsv (
            MkmAuthenticationData authentication,
            Action<MkmProduct> readSingleProductCallback) {
            var response = MakeRequest (
                authentication,
                "productlist",
                "");

            var doc = XDocument.Parse (response);
            var node = doc.Root.Element ("productsfile");

            var bytes = Convert.FromBase64String (node.Value);

            using (var decompressionStream = new GZipStream (new MemoryStream (bytes), CompressionMode.Decompress)) {
                // // using (var fileStream = File.Create (Path.Combine ("/home/michael", "temp.txt"))) {
                // //     decompressionStream.CopyTo (fileStream);
                // // }

                using (var decompressedStreamReader = new StreamReader (decompressionStream, Encoding.UTF8)) {

                    using (var csv = new CsvReader (decompressedStreamReader, CultureInfo.InvariantCulture)) {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.BadDataFound = (context) => {
                            // var debug = 0;
                        };

                        foreach (var product in csv.GetRecords<MkmProduct> ()) {
                            readSingleProductCallback?.Invoke (product);
                        }
                    }
                }
            }
        }

        public MkmProductMagic GetProductData (MkmAuthenticationData authentication, string productId) {
            var response = MakeRequest (
                authentication,
                $"products/{productId}",
                "");

            var doc = XDocument.Parse (response);
            var product = doc.Root.Element ("product");

            var priceGuide = product.Element ("priceGuide");

            return new MkmProductMagic () {
                ProductId = product?.Element ("idProduct")?.Value,
                    Name = product?.Element ("enName")?.Value,
                    WebSite = product?.Element ("website")?.Value,

                    PriceSell = priceGuide?.Element ("SELL")?.Value,
                    PriceLow = priceGuide?.Element ("LOW")?.Value,
                    PriceLowEx = priceGuide?.Element ("LOWEX")?.Value,
                    PriceLowFoil = priceGuide?.Element ("LOWFOIL")?.Value,
                    PriceAverage = priceGuide?.Element ("AVG")?.Value,
                    PriceTrend = priceGuide?.Element ("TREND")?.Value,
                    PriceTrendFoil = priceGuide?.Element ("TRENDFOIL")?.Value,
            };
        }

        public MkmProductMagic GetArticles (
            MkmAuthenticationData authentication,
            string productId,
            bool commercialOnly) {
            var parameters = new List<string> ();
            if (commercialOnly) {
                // parameters.Add($"userType=commercial");
                parameters.Add ("userType=private&idLanguage=1&minCondition=NM&start=0&maxResults=10");
            }

            var parameterString = "";
            if (parameters.Any ()) {
                parameterString = "?" + string.Join ("&", parameters);
            }

            var response = MakeRequest (
                authentication,
                $"articles/{productId}", parameterString);

            var doc = XDocument.Parse (response);
            var product = doc.Root.Element ("product");

            var priceGuide = product.Element ("priceGuide");

            return new MkmProductMagic () {
                ProductId = product?.Element ("idProduct")?.Value,
                    Name = product?.Element ("enName")?.Value,
                    WebSite = product?.Element ("website")?.Value,

                    PriceSell = priceGuide?.Element ("SELL")?.Value,
                    PriceLow = priceGuide?.Element ("LOW")?.Value,
                    PriceLowEx = priceGuide?.Element ("LOWEX")?.Value,
                    PriceLowFoil = priceGuide?.Element ("LOWFOIL")?.Value,
                    PriceAverage = priceGuide?.Element ("AVG")?.Value,
                    PriceTrend = priceGuide?.Element ("TREND")?.Value,
                    PriceTrendFoil = priceGuide?.Element ("TRENDFOIL")?.Value,
            };
        }

        internal string MakeRequest (
            MkmAuthenticationData authentication,
            string urlCommand,
            string parameters) {
            var method = "GET";
            var url = $"https://api.cardmarket.com/ws/v2.0/{urlCommand}";

            var request = WebRequest.CreateHttp (url + parameters) as HttpWebRequest;
            var header = new OAuthHeader (authentication);
            request.Headers.Add (HttpRequestHeader.Authorization, header.getAuthorizationHeader (method, url));
            request.Method = method;

            var response = request.GetResponse () as HttpWebResponse;
            var encoding = response.CharacterSet == "" ?
                Encoding.UTF8 :
                Encoding.GetEncoding (response.CharacterSet);

            using (var stream = response.GetResponseStream ()) {
                var reader = new StreamReader (stream, encoding);
                return reader.ReadToEnd ();
            }
        }
    }

    /// <summary>
    /// Class encapsulates tokens and secret to create OAuth signatures and return Authorization headers for web requests.
    /// </summary>
    internal class OAuthHeader {
        /// <summary>OAuth Signature Method</summary>
        protected string signatureMethod = "HMAC-SHA1";

        /// <summary>OAuth Version</summary>
        protected string version = "1.0";

        /// <summary>All Header params compiled into a Dictionary</summary>
        protected IDictionary<string, string> headerParams;

        private readonly MkmAuthenticationData _authentication;

        /// <summary>
        /// Constructor
        /// </summary>
        public OAuthHeader (MkmAuthenticationData authentication) {
            _authentication = authentication;
            // String nonce = Guid.NewGuid().ToString("n");
            var nonce = "53eb1f44909d6";

            // String timestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
            var timestamp = "1407917892";

            // Initialize all class members
            this.headerParams = new Dictionary<string, string> ();
            this.headerParams.Add ("oauth_consumer_key", _authentication.AppToken);
            this.headerParams.Add ("oauth_token", _authentication.AccessToken);
            this.headerParams.Add ("oauth_nonce", nonce);
            this.headerParams.Add ("oauth_timestamp", timestamp);
            this.headerParams.Add ("oauth_signature_method", this.signatureMethod);
            this.headerParams.Add ("oauth_version", this.version);
        }

        /// <summary>
        /// Pass request method and URI parameters to get the Authorization header value
        /// </summary>
        /// <param name="method">Request Method</param>
        /// <param name="url">Request URI</param>
        /// <returns>Authorization header value</returns>
        public string getAuthorizationHeader (string method, string url) {
            // Add the realm parameter to the header params
            this.headerParams.Add ("realm", url);

            // Start composing the base string from the method and request URI
            var baseString = method.ToUpper () +
                "&" +
                Uri.EscapeDataString (url) +
                "&";

            // Gather, encode, and sort the base string parameters
            var encodedParams = new SortedDictionary<string, string> ();
            foreach (var parameter in this.headerParams) {
                if (false == parameter.Key.Equals ("realm")) {
                    encodedParams.Add (Uri.EscapeDataString (parameter.Key), Uri.EscapeDataString (parameter.Value));
                }
            }

            // Expand the base string by the encoded parameter=value pairs
            var paramStrings = new List<string> ();
            foreach (var parameter in encodedParams) {
                paramStrings.Add (parameter.Key + "=" + parameter.Value);
            }
            var paramString = Uri.EscapeDataString (string.Join<string> ("&", paramStrings));
            baseString += paramString;

            // Create the OAuth signature
            var signatureKey = Uri.EscapeDataString (_authentication.AppSecret) + "&" + Uri.EscapeDataString (_authentication.AccessSecret);
            var hasher = HMACSHA1.Create ("HMACSHA1");
            hasher.Key = Encoding.UTF8.GetBytes (signatureKey);
            var rawSignature = hasher.ComputeHash (Encoding.UTF8.GetBytes (baseString));
            var oAuthSignature = Convert.ToBase64String (rawSignature);

            // Include the OAuth signature parameter in the header parameters array
            this.headerParams.Add ("oauth_signature", oAuthSignature);

            // Construct the header string
            var headerParamStrings = new List<string> ();
            foreach (var parameter in this.headerParams) {
                headerParamStrings.Add (parameter.Key + "=\"" + parameter.Value + "\"");
            }
            var authHeader = "OAuth " + string.Join<string> (", ", headerParamStrings);

            return authHeader;
        }
    }
}