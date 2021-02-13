using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MtgJson.JsonModels;

namespace MtgJson
{
    public interface IMtgJsonService
    {
        void DownloadPriceData(
            FileInfo localFile,
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter);

        Task DownloadPriceDataAsync(
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter);
    }
}