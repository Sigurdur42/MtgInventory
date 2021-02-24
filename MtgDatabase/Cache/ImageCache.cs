using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MtgDatabase.Models;
using MtgDatabase.Scryfall;

namespace MtgDatabase.Cache
{
    public class ImageCache : IImageCache
    {
        private readonly GoodCitizenAutoSleep _goodCitizenAutoSleep = new GoodCitizenAutoSleep();
        private readonly ILogger<ImageCache> _logger;
        private DirectoryInfo? _baseFolder;

        private bool _isInitialized;

        public ImageCache(ILogger<ImageCache> logger)
        {
            _logger = logger;
        }

        public bool IsInitialized => _isInitialized;

        public string DownloadSingleFile(QueryableMagicCard card)
        {
            if (!IsInitialized)
            {
                _logger.LogTrace($"Ignoring {nameof(DownloadSingleFile)} call as we are not initialized");
                return card.Images.Normal;
            }

            var localName = GetLocalName(card);
            if (File.Exists(localName))
            {
                return localName;
            }

            // Card does not exist locally - queue for download and return original uri

            _logger.LogInformation($"Downloading image {new FileInfo(localName).Name}...");
            DownloadSingleFile(
                localName,
                card.Images.Normal);

            if (File.Exists(localName))
            {
                return localName;
            }

            return card.Images.Normal;
        }

        public string GetCachedImage(QueryableMagicCard card)
        {
            return DownloadSingleFile(card);
        }

        public void Initialize(DirectoryInfo baseCacheFolder)
        {
            _baseFolder = baseCacheFolder;
            _logger.LogInformation($"Initializing image cache in {baseCacheFolder.FullName}");
            _isInitialized = true;
        }

        public void QueueForDownload(QueryableMagicCard[] cards)
        {
            if (!IsInitialized)
            {
                _logger.LogTrace($"Ignoring {nameof(QueueForDownload)} call as we are not initialized");
                return;
            }

            Task.Factory.StartNew(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation($"Starting download for {cards.Length} cards...");
                foreach (var card in cards)
                {
                    DownloadSingleFile(card);
                }

                stopwatch.Stop();
                _logger.LogInformation($"Finished download of {cards.Length} cards in {stopwatch.Elapsed}.");
            });
        }

        private void DownloadSingleFile(
            string localFileName,
            string remoteUri)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteUri))
                {
                    return;
                }

                var info = new FileInfo(localFileName);
                if (!info.Directory.Exists)
                {
                    info.Directory.Create();
                }

                _goodCitizenAutoSleep.AutoSleep();
                using var client = new WebClient();
                client.DownloadFile(remoteUri, localFileName);
            }
            catch (Exception error)
            {
                _logger.LogError($"Error downloading {remoteUri}: {error.Message}", error);
            }
        }

        private string GetLocalName(QueryableMagicCard card)
        {
            var uniqueId = (card.UniqueId ?? "")
                .Replace(":", "_")
                .Replace(".", "_")
                .Replace("/", "_")
                .Replace("*", "_STAR_")
                .Replace(",", "_");

            var patchedSetCode = card.SetCode
                .Replace("con", "conflux");

            return Path.Combine(_baseFolder!.FullName, "ImageCache", "normal", patchedSetCode, $"{uniqueId}.png");
        }
    }
}