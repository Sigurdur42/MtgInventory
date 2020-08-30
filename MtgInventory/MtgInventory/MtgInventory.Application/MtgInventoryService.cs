﻿using MkmApi;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Log.Information($"{nameof(Initialize)}: Initializing application service");

            var reader = new AuthenticationReader();

            _mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            Log.Debug($"{nameof(Initialize)}: Loading MKM authentication data from '{_mkmAuthenticationDataFile.FullName}'...");

            MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _mkmRequest = new MkmRequest(MkmAuthenticationData);

            _cardDatabase.Initialize(SystemFolders.BaseFolder);
            MkmProductsSummary = $"{_cardDatabase.MkmProductInfo.Count()} products in database";
        }

        public void ShutDown()
        {
            Log.Information($"{nameof(ShutDown)}: Shutting down application service");

            _cardDatabase.Dispose();
        }

        public void DownloadMkmProducts()
        {
            Log.Debug($"{nameof(DownloadMkmProducts)}: Loading expansions...");

            var expansions = _mkmRequest.GetExpansions(1);
            _cardDatabase.InsertExpansions(expansions);

            Log.Debug($"{nameof(DownloadMkmProducts)}: Loading products...");
            using var products = _mkmRequest.GetProductsAsCsv();

            Log.Debug($"{nameof(DownloadMkmProducts)}: Inserting products into database...");
            _cardDatabase.InsertProductInfo(products.Products, expansions);
        }

        public void OpenMkmProductPage(MkmProductInfo product)
        {
            if (product == null)
            {
                return;
            }

            var prefix = $"{nameof(OpenMkmProductPage)}({product.Id} {product.Name} {product.ExpansionCode})";

            if (string.IsNullOrEmpty(product.MkmProductUrl))
            {
                Log.Information($"{prefix}: Downloading additional info...");

                // We need to download the product details first
                var p = _mkmRequest.GetProductData(product.Id);
                product.UpdateFromProduct(p);

                _cardDatabase.MkmProductInfo.Update(product);
            }

            // Now open a browser with the url
            Log.Debug($"{prefix}: Opening MKM product page...");
            Browser.OpenBrowser(product.MkmProductUrl);
        }

        public IEnumerable<MkmProductInfo> MkmFindProductsByName(string name)
        {
            Log.Debug($"{nameof(MkmFindProductsByName)}: {name}");
            return _cardDatabase.MkmProductInfo
                .Query()
                .Where(p => p.CategoryId == 1)
                .Where(p => p.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                .ToList()
                .OrderBy(p => p.Name)
                .ThenBy(p => p.ExpansionName)
                .ToList();
        }
    }
}