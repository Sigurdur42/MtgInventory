using System;
using System.Collections.Generic;
using System.IO;
using MtgJson.CsvModels;
using MtgJson.JsonModels;

namespace MtgJson
{
    public interface ILiteDbService : IDisposable
    {
        void Configure(DirectoryInfo folder);

        void DeleteExistingDatabase();

        void OnPriceDataBatchLoaded(IEnumerable<JsonCardPrice> loadedBatch);

        bool OnPriceDataHeaderLoaded(JsonMeta metaData);
        void OnLegalitiyBatchLoaded(IEnumerable<CsvLegalities> loadedBatch);
        void OnForeignDataBatchLoaded(IEnumerable<CsvForeignData> loadedBatch);
        void OnCardDataBatchLoaded(IEnumerable<CsvCard> loadedBatch);
        void OnSetDataBatchLoaded(IEnumerable<CsvSet> loadedBatch);

        void WaitOnInsertTasksAndClear();
    }
}