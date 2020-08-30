using MkmApi;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MtgInventory.Service
{
    /// <summary>
    /// This is the main service class which encapsulates all functionality
    /// </summary>
    public sealed class MtgInventoryService : IDisposable
    {
        private readonly CardDatabase _cardDatabase;
        private MkmRequest _mkmRequest;
        private FileInfo _mkmAuthenticationDataFile;

        public MtgInventoryService()
        {
            SystemFolders = new SystemFolders();
            _cardDatabase = new CardDatabase();
        }

        public SystemFolders SystemFolders { get; }

        public MkmAuthenticationData MkmAuthenticationData { get; private set; }

        public string MkmProductsSummary { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize()
        {
            var reader = new AuthenticationReader();

            _mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _mkmRequest = new MkmRequest(MkmAuthenticationData);

            _cardDatabase.Initialize(SystemFolders.BaseFolder);
            MkmProductsSummary = $"{_cardDatabase.MkmProductInfo.Count()} products in database";

            // TODO: Implement async init
            // Loading of database etc.

            // TODO: Implement reading all products from MKM and put it into database
        }

        public void ShutDown()
        {
            _cardDatabase.Dispose();
        }

        public void DownloadMkmProducts()
        {
            var expansions = _mkmRequest.GetExpansions(1);
            _cardDatabase.InsertExpansions(expansions);

            using var products = _mkmRequest.GetProductsAsCsv();

            _cardDatabase.InsertProductInfo(products.Products, expansions);
        }

        public void OpenMkmProductPage(MkmProductInfo product)
        {
            if (product == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(product.MkmProductUrl))
            {
                // We need to download the product details first
                var p = _mkmRequest.GetProductData(product.Id);
                product.UpdateFromProduct(p);

                _cardDatabase.MkmProductInfo.Update(product);
            }

            // Now open a browser with the url
            Browser.OpenBrowser(product.MkmProductUrl);
        }

        public IEnumerable<MkmProductInfo> MkmFindProductsByName(string name)
        {
            return _cardDatabase.MkmProductInfo
                .Query()
                .Where(p => p.CategoryId == 1)
                .Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(p => p.Name)
                .ToList();
        }
    }
}