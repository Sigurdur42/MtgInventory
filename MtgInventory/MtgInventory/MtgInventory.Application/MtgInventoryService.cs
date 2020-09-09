using MkmApi;
using MtgInventory.Service.Database;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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

        public IApiCallStatistic MkmApiCallStatistic { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize(IApiCallStatistic mkmApiCallStatistic)
        {
            Log.Information($"{nameof(Initialize)}: Initializing application service");

            var reader = new AuthenticationReader();

            _mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            Log.Debug($"{nameof(Initialize)}: Loading MKM authentication data from '{_mkmAuthenticationDataFile.FullName}'...");

            MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _cardDatabase.Initialize(SystemFolders.BaseFolder);
            
            MkmApiCallStatistic = mkmApiCallStatistic;
            var fromDatabase = _cardDatabase.GetMkmCallStatistic();
            mkmApiCallStatistic.CountToday = fromDatabase.CountToday;
            mkmApiCallStatistic.CountTotal = fromDatabase.CountTotal;
            mkmApiCallStatistic.Today = fromDatabase.Today;
            mkmApiCallStatistic.Id = fromDatabase.Id;

            MkmProductsSummary = $"{_cardDatabase.MkmProductInfo.Count()} products in database";

            _mkmRequest = new MkmRequest(MkmAuthenticationData, MkmApiCallStatistic);
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

            _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
        }

        public void OpenMkmProductPage(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                Log.Warning($"Cannot find product with empty MKM id");
            }

            var found = _cardDatabase.MkmProductInfo.Query().Where(p => p.Id == productId).FirstOrDefault();
            if (found == null)
            {
                Log.Warning($"Cannot find product with MKM id {productId}");
                return;
            }

            OpenMkmProductPage(found);
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

                _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
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

        public void EnrichDeckListWithDetails(DeckList deckList)
        {
            Log.Debug($"{nameof(EnrichDeckListWithDetails)} for deck {deckList.Name}");
            var expansions = _cardDatabase.MkmExpansion
                .Query()
                .OrderByDescending(e => e.ReleaseDateParsed)
                .ToList();

            foreach (var card in deckList.Mainboard.Where(c => c.MkmId == null))
            {
                var found = _cardDatabase.MkmProductInfo
                    .Query()
                    .Where(c => c.Name.Equals(card.Name))
                    .Where(c => c.ExpansionCode != null)
                    .ToList();

                var validExpansions = expansions
                    .Where(e => found.Any(f => f.ExpansionCode == e.Abbreviation))
                    .OrderByDescending(e => e.ReleaseDateParsed)
                    .ToArray();

                if (found.Any())
                {
                    var use = found.First(f => f.ExpansionCode == validExpansions.First().Abbreviation);
                    card.MkmId = use.Id;
                    card.SetCode = use.ExpansionCode;
                    card.SetName = use.ExpansionName;
                }
            }
        }

        public IEnumerable<MkmStockItemExtended> DownloadMkmStock()
        {
            Log.Debug($"{nameof(DownloadMkmStock)} now...");

            var result = _mkmRequest
                .GetStockAsCsv()
                .Select(s => new MkmStockItemExtended(s))
                .ToArray();

            _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);

            Log.Debug($"{nameof(DownloadMkmStock)} loaded {result.Length} items");

            return result;
        }
    }
}