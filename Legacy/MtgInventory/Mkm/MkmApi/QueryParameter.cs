using System;
using System.Diagnostics;

namespace MkmApi
{
    [DebuggerDisplay("{Name}={Value}")]
    public class QueryParameter
    {
        public QueryParameter(
            string name,
            string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }

        
    }
}