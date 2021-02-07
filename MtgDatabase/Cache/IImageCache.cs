using System.IO;
using MtgDatabase.Models;

namespace MtgDatabase.Cache
{
    public interface IImageCache
    {
        bool IsInitialized { get; }

        string GetCachedImage(QueryableMagicCard card);
        void Initialize(DirectoryInfo baseCacheFolder);
        void QueueForDownload(QueryableMagicCard[] cards);
    }
}