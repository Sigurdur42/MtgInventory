using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall
{
    public class RequestResult
    {           
        public bool Success { get; set; }
        public string JsonResult { get; set; }
        public int StatusCode { get; set; }
    }
}
