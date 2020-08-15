namespace MkmApi
{
    public sealed class MkmAuthenticationData
    {
        /// <summary>App Token</summary>
        public string AppToken { get; set; }

        /// <summary>App Secret</summary>
        public string AppSecret { get; set; }

        /// <summary>Access Token (Class should also implement an AccessToken property to set the value)</summary>
        public string AccessToken { get; set; }

        /// <summary>Access Token Secret (Class should also implement an AccessToken property to set the value)</summary>
        public string AccessSecret { get; set; }
    }
}