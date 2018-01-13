using System.Net.Http;

namespace MtgScryfall
{
    public static class RequestResultExtension
    {
        public static RequestResult CreateResult(this HttpResponseMessage response)
        {
            var result = new RequestResult
            {
                StatusCode = (int)response.StatusCode,
                Success = response.IsSuccessStatusCode,
            };

            if (response.IsSuccessStatusCode)
            {
                result.JsonResult = response.Content.ReadAsStringAsync().Result;
            }

            return result;
        }
    }
}