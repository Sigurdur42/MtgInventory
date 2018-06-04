using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public class MagicSetInfo
    {
        public bool IsDigitalOnly { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string SvgUrl { get; set; }
    }
}