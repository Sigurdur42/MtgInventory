using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace MtgBinders.Domain.Services.Images
{
    internal class MtgImageCache : IMtgImageCache
    {
        private readonly string _cardFolder;
        private readonly ILogger _logger;

        public MtgImageCache(
            IBinderDomainConfigurationProvider configuration,
            ILoggerFactory loggerFactory)
        {
            var cacheBaseFolder = Path.Combine(configuration.AppDataFolder, "ImageCache");
            _cardFolder = Path.Combine(cacheBaseFolder, "Cards");

            _logger = loggerFactory.CreateLogger(nameof(MtgImageCache));
        }

        public string GetImageFile(MtgFullCard card)
        {
            if (card == null)
            {
                return null;
            }

            var fileNamePart = string.IsNullOrWhiteSpace(card.CollectorNumber) ? card.Name : card.CollectorNumber;
            fileNamePart = fileNamePart.Replace("*", "_");
            var localFileName = Path.Combine(_cardFolder, card.SetCode, $"{fileNamePart}.png");

            if (File.Exists(localFileName))
            {
                return localFileName;
            }

            var folder = new FileInfo(localFileName).Directory;
            if (folder != null && !folder.Exists)
            {
                folder.Create();
            }

            if (string.IsNullOrWhiteSpace(card.ImageLarge))
            {
                _logger.LogError($"Error downloading image for '{card.Name} ({card.SetCode})': No image path.");

                return null;
            }

            var result = DownloadImage(card.ImageLarge, localFileName);
            return result.IsSuccessStatusCode ? localFileName : null;
        }

        public int DownloadMissingImages(IEnumerable<MtgFullCard> cards)
        {
            var local = cards.ToArray();
            return local.Select(i=>
            {
                Thread.Sleep(80);
                return GetImageFile(i);
            }).Count(i => i != null); 
        }

        private HttpResponseMessage DownloadImage(string imageUrl, string localFile)
        {
            using (var client = new HttpClient())
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogDebug($"Start downloading image {imageUrl}...");

                var result = client.GetAsync(imageUrl).Result;
                if (result.IsSuccessStatusCode)
                {
                    using (var stream = File.Create(localFile, 64, FileOptions.None))
                    {
                        var task = result.Content.CopyToAsync(stream);
                        task.Wait();
                    }
                }
                else
                {
                    _logger.LogError($"Error downloading image from '{imageUrl}' to '{localFile}': {result.ReasonPhrase}");
                }

                stopwatch.Stop();
                _logger.LogDebug($"Finished downloading image {imageUrl} with code {result.StatusCode} in {stopwatch.Elapsed}.");
                return result;
            }
        }
    }
}