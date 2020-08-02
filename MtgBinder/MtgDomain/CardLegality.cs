using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MtgDomain
{
    [DebuggerDisplay("{Format} - {Legality}")]
    public class CardLegality
    {
        public string Format { get; set; }

        public string Legalitiy { get; set; }
    }
}
