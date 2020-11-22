using System;
using System.IO;
using System.Linq;
using System.Net;
using MtgInventory.Service.Models;
using ScryfallApiServices.Models;

namespace MtgInventory.Service
{
    public class AutoDownloadImageCache
    {
        private readonly DirectoryInfo _baseFolder;

        public AutoDownloadImageCache(
            DirectoryInfo baseFolder)
        {
            _baseFolder = new DirectoryInfo(Path.Combine(baseFolder.FullName, "ImageCache"));
            Instance = this;
        }

        public static AutoDownloadImageCache? Instance { get; private set; }

        public FileInfo? GetOrDownload(
            DetailedMagicCard card,
            string category)
        {
            var localFile = BuildCacheFileName(card, category);
            if (localFile.Exists)
            {
                return localFile;
            }

            // Log.Debug($"Downloading '{category}' image for {card.SetCode} {card.NameEn}");
            // Need to download it
            var uri = GetUrl(card, category);
            if (!uri.IsValid)
            {
                // Log.Warning($"{card.SetCode} {card.NameEn} - no image uri set - cannot download.");
                return null;
            }

            var exists = localFile.Directory?.Exists ?? false;
            if (!exists)
            {
                localFile.Directory?.Create();
            }

            DownloadRemoteImageFile(uri.Uri, localFile.FullName);

            localFile.Refresh();
            if (localFile.Exists)
            {
                return localFile;
            }

            return null;
        }

        internal FileInfo BuildCacheFileName(
            DetailedMagicCard card,
            string category)
        {
            var categoryFolder = category;
            if (string.IsNullOrWhiteSpace(category))
            {
                categoryFolder = "normal";
            }

            var cardId = card.ScryfallId.ToString();
            var sourceFolder = "Scryfall";
            if (card.ScryfallId == Guid.Empty)
            {
                sourceFolder = "MKM";
                cardId = card.MkmId;
            }

            return new FileInfo(Path.Combine(
                _baseFolder.FullName,
                categoryFolder,
                card.SetCode,
                sourceFolder,
                cardId + ".png"));
        }

        internal ImageLinkUri GetUrl(
            DetailedMagicCard card,
            string category)
        {
            if (card.ScryfallId == Guid.Empty)
            {
                // This is an MKM only card
                return card.MkmImages.FirstOrDefault() ?? new ImageLinkUri();
            }
            else
            {
                var found = card.ScryfallImages
                    .FirstOrDefault(i => i.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase));

                if (found == null)
                {
                    // try to find the normal one:
                    found = card.ScryfallImages
                        .FirstOrDefault(i => i.Category.Equals("normal", StringComparison.InvariantCultureIgnoreCase));
                }

                return found ?? new ImageLinkUri();
            }
        }

        private static void DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception error)
            {
                // Log.Error($"Error downloading {uri} to {fileName}: {error}");
                return;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.Moved ||
                 response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // if the remote file was found, download it
                    using Stream inputStream = response.GetResponseStream();
                    using Stream outputStream = File.OpenWrite(fileName);

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                catch (Exception e)
                {
                    // Log.Error($"Cannot download image to {fileName}: {e}");
                }
            }
        }
    }
}