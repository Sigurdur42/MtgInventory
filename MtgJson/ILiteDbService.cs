using System;
using System.Collections.Generic;
using System.IO;
using MtgJson.JsonModels;

namespace MtgJson
{
    public interface ILiteDbService : IDisposable
    {
        void Configure(DirectoryInfo folder);
        bool OnPriceDataHeaderLoaded(JsonMeta metaData);
        void OnPriceDataBatchLoaded(IEnumerable<JsonCardPrice> loadedBatch);
    }
}