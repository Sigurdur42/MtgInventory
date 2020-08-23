using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace MkmApi
{
    public sealed class ProductCsvData : IDisposable
    {
        private readonly GZipStream _decompressionStream;
        private readonly StreamReader _decompressedStreamReader;
        private readonly CsvReader _csvReader;

        internal ProductCsvData(
            string response)
        {
            var doc = XDocument.Parse(response);
            var node = doc.Root.Element("productsfile");

            var bytes = Convert.FromBase64String(node.Value);

            _decompressionStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);

            //// using (var fileStream = File.Create (Path.Combine ("/home/michael", "temp.txt"))) {
            ////     decompressionStream.CopyTo (fileStream);
            //// }

            _decompressedStreamReader = new StreamReader(_decompressionStream, Encoding.UTF8);

            _csvReader = new CsvReader(_decompressedStreamReader, CultureInfo.InvariantCulture);

            _csvReader.Configuration.HasHeaderRecord = true;
            _csvReader.Configuration.Delimiter = ",";
            _csvReader.Configuration.BadDataFound = (context) =>
            {
                // var debug = 0;
            };

            Products = _csvReader.GetRecords<ProductInfo>();
        }

        public IEnumerable<ProductInfo> Products { get; private set; }

        public void Dispose()
        {
            _csvReader?.Dispose();
            _decompressedStreamReader?.Dispose();
            _decompressionStream?.Dispose();
        }
    }
}