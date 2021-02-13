using System;
using System.Collections.Generic;
using System.IO;
using MtgJson.JsonModels;

namespace MtgJson
{
    public interface ILiteDbService : IDisposable
    {
        void Configure(DirectoryInfo folder);

        void DeleteExistingDatabase();

        void OnPriceDataBatchLoaded(IEnumerable<JsonCardPrice> loadedBatch);

        bool OnPriceDataHeaderLoaded(JsonMeta metaData);

        void WaitOnInsertTasksAndClear();
    }
}