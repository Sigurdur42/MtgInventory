using System;
using System.Collections.Generic;
using System.Text;

namespace MtgJson.Sqlite.Models
{
    public class DbPrice
    {
        public int? Id { get; set; }
        public string Uuid { get; set; } = "";

        public string Date { get; set; } = "";
        public decimal? MkmNormal { get; set; }
        public decimal? MkmFoil { get; set; }
    }
}
