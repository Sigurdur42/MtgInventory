using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinder.Domain.Mkm
{
    public sealed class MkmAuthentication
    {
        /// <summary>App Token</summary>
        public string AppToken { get; set; } = "8mVibDpu6NFudcfN";

        /// <summary>App Secret</summary>
        public string AppSecret { get; set; } = "kbck26RTOw7IOI243qgg2fkuw9b20r27";

        /// <summary>Access Token (Class should also implement an AccessToken property to set the value)</summary>
        public string AccessToken { get; set; } = "clduU54s8ZsXyUukDbapck0W3Yq1HO3B";

        /// <summary>Access Token Secret (Class should also implement an AccessToken property to set the value)</summary>
        public string AccessSecret { get; set; } = "V4f5zu2kFzlJ7wtcocmeQygTM5iTh6Cc";
    }
}
