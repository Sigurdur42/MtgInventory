using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MtgJson.CsvModels;
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

        void DownloadAllPrintingsZip(
            FileInfo localFile,
            Func<CsvMeta, bool> headerLoaded,
            Func<CsvSet[], bool> setsLoaded,
            Func<IEnumerable<CsvCard>, bool> cardsLoaded,
            Func<IEnumerable<CsvForeignData>, bool> foreignDataLoaded,
            Func<IEnumerable<CsvLegalities>, bool> legalitiesLoaded);
    }
}