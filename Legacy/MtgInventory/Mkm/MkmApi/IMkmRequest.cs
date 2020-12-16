using System.Collections.Generic;
using MkmApi.Entities;

namespace MkmApi
{
    public interface IMkmRequest
    {
        IEnumerable<Product> FindProducts(
            MkmAuthenticationData authenticationData,
            string productName,
            bool searchExact);

        IEnumerable<Article> GetArticles(
            MkmAuthenticationData authenticationData,
            string productId,
            bool commercialOnly,
            IEnumerable<QueryParameter> queryParameters);

        IEnumerable<Expansion> GetExpansions(MkmAuthenticationData authenticationData, int gameId);

        IEnumerable<Game> GetGames(MkmAuthenticationData authenticationData);

        Product GetProductData(MkmAuthenticationData authenticationData, string productId);

        ProductCsvData GetProductsAsCsv(MkmAuthenticationData authenticationData);

        IEnumerable<MkmStockItem> GetStockAsCsv(MkmAuthenticationData authenticationData);
    }
}