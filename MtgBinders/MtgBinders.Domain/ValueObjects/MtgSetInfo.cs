using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public class MtgSetInfo
    {
        public bool IsDigitalOnly { get; set; }
        public bool IsFoilOnly { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string SvgUrl { get; set; }
        public int NumberOfCards { get; set; }
        public string ReleasDate { get; set; }
    }
}