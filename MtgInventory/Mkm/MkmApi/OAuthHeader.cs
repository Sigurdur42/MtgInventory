using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MkmApi
{
    /// <summary>
    /// Class encapsulates tokens and secret to create OAuth signatures and return Authorization headers for web requests.
    /// </summary>
    internal class OAuthHeader
    {
        /// <summary>All Header params compiled into a Dictionary</summary>
        protected IDictionary<string, string> headerParams;

        /// <summary>OAuth Signature Method</summary>
        protected string signatureMethod = "HMAC-SHA1";

        /// <summary>OAuth Version</summary>
        protected string version = "1.0";

        private readonly MkmAuthenticationData _authentication;

        /// <summary>
        /// Constructor
        /// </summary>
        public OAuthHeader(MkmAuthenticationData authentication)
        {
            _authentication = authentication;
            // var nonce = Guid.NewGuid().ToString("n");
            var nonce = "53eb1f44909d6";

            // var timestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
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
        /// <param name="urlWithoutQueryParameters">Request URI</param>
        /// <param name="queryParameters">query arguments</param>
        /// <returns>Authorization header value</returns>
        public string GetAuthorizationHeader(
            string method, 
            string urlWithoutQueryParameters, 
            IEnumerable<QueryParameter> queryParameters)
        {
            // Add the realm parameter to the header params
            this.headerParams.Add("realm", urlWithoutQueryParameters);

            // Start composing the base string from the method and request URI
            var baseString = method.ToUpper() +
                "&" +
                Uri.EscapeDataString(urlWithoutQueryParameters) +
                "&";

            // Gather, encode, and sort the base string parameters
            var encodedParams = new SortedDictionary<string, string>();
            foreach (var parameter in this.headerParams)
            {
                if (!parameter.Key.Equals("realm"))
                {
                    encodedParams.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
                }
            }

            foreach (var param in queryParameters)
            {
                encodedParams.Add(Uri.EscapeDataString(param.Name), Uri.EscapeDataString(param.Value));
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