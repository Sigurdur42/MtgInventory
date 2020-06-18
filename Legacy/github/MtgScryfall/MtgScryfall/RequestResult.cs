using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall
{
    public class RequestResult
    {
        public string JsonResult { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
    }
}