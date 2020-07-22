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

namespace MtgBinder.Domain.Mkm
{
    public class MkmRequest
    {
        public IEnumerable<MkmStockItem> GetStockAsCsv(MkmAuthentication authentication)
        {
            var response = MakeRequest(
                authentication,
                "stock/file");

            var doc = XDocument.Parse(response);
            var stockNode = doc.Root.Element("stock");

            var bytes = Convert.FromBase64String(stockNode.Value);
            // var contents = new StreamContent(new MemoryStream(bytes));

            using (var decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                using (var decompressedStreamReader = new StreamReader(decompressionStream, Encoding.UTF8))
                {
                    using (var csv = new CsvReader(decompressedStreamReader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Delimiter = ";";
                        csv.Configuration.BadDataFound = (context) =>
                        {
                            var debug = 0;
                        };
                        return csv.GetRecords<MkmStockItem>().ToList();
                    }

                    // Do something with the value
                }

                // Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
            }
        }

        internal string MakeRequest(
            MkmAuthentication authentication,
            string urlCommand)
        {
            var method = "GET";
            // var url = "https://api.cardmarket.com/ws/v2.0/account";
            var url = $"https://api.cardmarket.com/ws/v2.0/{urlCommand}";

            var request = WebRequest.CreateHttp(url) as HttpWebRequest;
            var header = new OAuthHeader(authentication);
            request.Headers.Add(HttpRequestHeader.Authorization, header.getAuthorizationHeader(method, url));
            request.Method = method;

            var response = request.GetResponse() as HttpWebResponse;
            var encoding = response.CharacterSet == ""
                ? Encoding.UTF8
                : Encoding.GetEncoding(response.CharacterSet);

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Class encapsulates tokens and secret to create OAuth signatures and return Authorization headers for web requests.
    /// </summary>
    internal class OAuthHeader
    {
        /// <summary>OAuth Signature Method</summary>
        protected string signatureMethod = "HMAC-SHA1";

        /// <summary>OAuth Version</summary>
        protected string version = "1.0";

        /// <summary>All Header params compiled into a Dictionary</summary>
        protected IDictionary<string, string> headerParams;

        private readonly MkmAuthentication _authentication;

        /// <summary>
        /// Constructor
        /// </summary>
        public OAuthHeader(MkmAuthentication authentication)
        {
            _authentication = authentication;
            // String nonce = Guid.NewGuid().ToString("n");
            var nonce = "53eb1f44909d6";

            // String timestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
            var timestamp = "1407917892";

            // Initialize all class members
            this.headerParams = new Dictionary<string, string>();
            this.headerParams.Add("oauth_consumer_key", _authentication.AppToken);
            this.headerParams.Add("oauth_token", _authentication.AccessToken);
            this.headerParams.Add("oauth_nonce", nonce);
            this.headerParams.Add("oauth_timestamp", timestamp);
            this.headerParams.Add("oauth_signature_method", this.signatureMethod);
            this.headerParams.Add("oauth_version", this.version);
        }

        /// <summary>
        /// Pass request method and URI parameters to get the Authorization header value
        /// </summary>
        /// <param name="method">Request Method</param>
        /// <param name="url">Request URI</param>
        /// <returns>Authorization header value</returns>
        public string getAuthorizationHeader(string method, string url)
        {
            // Add the realm parameter to the header params
            this.headerParams.Add("realm", url);

            // Start composing the base string from the method and request URI
            var baseString = method.ToUpper()
                             + "&"
                             + Uri.EscapeDataString(url)
                             + "&";

            // Gather, encode, and sort the base string parameters
            var encodedParams = new SortedDictionary<string, string>();
            foreach (var parameter in this.headerParams)
            {
                if (false == parameter.Key.Equals("realm"))
                {
                    encodedParams.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
                }
            }

            // Expand the base string by the encoded parameter=value pairs
            var paramStrings = new List<string>();
            foreach (var parameter in encodedParams)
            {
                paramStrings.Add(parameter.Key + "=" + parameter.Value);
            }
            var paramString = Uri.EscapeDataString(string.Join<string>("&", paramStrings));
            baseString += paramString;

            // Create the OAuth signature
            var signatureKey = Uri.EscapeDataString(_authentication.AppSecret) + "&" + Uri.EscapeDataString(_authentication.AccessSecret);
            var hasher = HMACSHA1.Create("HMACSHA1");
            hasher.Key = Encoding.UTF8.GetBytes(signatureKey);
            var rawSignature = hasher.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var oAuthSignature = Convert.ToBase64String(rawSignature);

            // Include the OAuth signature parameter in the header parameters array
            this.headerParams.Add("oauth_signature", oAuthSignature);

            // Construct the header string
            var headerParamStrings = new List<string>();
            foreach (var parameter in this.headerParams)
            {
                headerParamStrings.Add(parameter.Key + "=\"" + parameter.Value + "\"");
            }
            var authHeader = "OAuth " + string.Join<string>(", ", headerParamStrings);

            return authHeader;
        }
    }
}