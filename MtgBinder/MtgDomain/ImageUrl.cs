using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MtgDomain
{
    [DebuggerDisplay("{Key} - {Url}")]
    public class ImageUrl
    {
        public string Key { get; set; }
        public Uri Url { get; set; }
    }
}
