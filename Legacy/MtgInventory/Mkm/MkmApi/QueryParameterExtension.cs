using System;
using System.Collections.Generic;
using System.Linq;

namespace MkmApi
{
    internal static class QueryParameterExtension
    {
        public static string GenerateQueryString(this IEnumerable<QueryParameter> queryParameters)
        {
            if (queryParameters == null || !queryParameters.Any())
            {
                return "";
            }

            return "?" + string.Join("&", queryParameters.Select(q => $"{Uri.EscapeDataString(q.Name)}={Uri.EscapeDataString(q.Value)}"));
        }
    }
}