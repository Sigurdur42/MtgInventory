using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall.Models
{
    public class SetData
    {
        public bool IsDigitalOnly { get; set; }
        public int NumberOfCards { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string SetType { get; set; }
        public string SvgUrl { get; set; }
        public bool IsFoilOnly { get; set; }
        public string ReleaseDate { get; set; }
    }
}