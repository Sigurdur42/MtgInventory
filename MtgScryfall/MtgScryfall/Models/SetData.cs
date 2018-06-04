using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall.Models
{
    public class SetData
    {
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string SetType { get; set; }
        public bool IsDigitalOnly { get; set; }
        public string SvgUrl { get; set; }
    }
}