using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MtgJson.Sqlite.Models;
using MtgJson.Sqlite.Repository;

namespace MtgJson.Sqlite
{
    public class MtgJsonMirrorIntoSqliteService : IMtgJsonMirrorIntoSqliteService
    {
        private readonly ILogger<MtgJsonMirrorIntoSqliteService> _logger;
        private readonly IMtgJsonService _mtgJsonService;

        public MtgJsonMirrorIntoSqliteService(
            ILogger<MtgJsonMirrorIntoSqliteService> logger,
            IMtgJsonService mtgJsonService)
        {
            _logger = logger;
            _mtgJsonService = mtgJsonService;
        }

        public async Task CreateLocalSqliteMirror(
            FileInfo targetFile)
        {
            CopyReferenceDatabase(targetFile);

            // https://dotnetcorecentral.com/blog/how-to-use-sqlite-with-dapper/

            using var connection = new DbConnection(targetFile);

            var cards = await DownloadDatabase(connection);

            var insertStatement = @"INSERT INTO cards (Name, NameDE, Uuid, ScryfallId, TypeLine, OracleText, SetCode, CardMarketId, CollectorNumber) VALUES (@NameName, @NameDE, @Uuid, @ScryfallId, @TypeLine, @OracleText, @SetCode, @CardMarketId, @CollectorNumber);";

            foreach (var card in cards)
            {
                await connection.Connection.ExecuteAsync(insertStatement, card);
            }

            // var exists = connection.QueryTableExists("cards");

            // TODO: continue here
        }

        private void CopyReferenceDatabase(FileInfo targetFile)
        {
            var source = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "SqliteMirror.sqlite");

            if (!targetFile.Directory.Exists)
            {
                targetFile.Directory.Create();
            }

            File.Copy(source, targetFile.FullName, true);
        }

        private async Task<IList<DbCards>> DownloadDatabase(DbConnection connection)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                _logger.LogInformation("Downloading AllPrintings now");
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync("https://mtgjson.com/api/v5/AllPrintingsCSVFiles.zip");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to download file: {response.StatusCode}");
                    return new List<DbCards>();
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await using var fileStream = File.Create(tempFile);
                    await stream.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Downloaded AllPrintingsCSVFiles to temp folder - starting analysis");

                var cardFactory = new DbCardFactory();

                _mtgJsonService.DownloadAllPrintingsZip(
                    new FileInfo(tempFile),
                    (header) =>
                    {
                        // TODO
                        // cardDate = DateTime.Parse(header.Date, );
                        return true;
                        // return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
                    },
                    (sets) =>
                    {
                        cardFactory.LoadedSets = sets.ToArray();
                        return true;
                    },
                    (cards) =>
                    {
                        cardFactory.AllCards = cards.ToDictionary(c => c.Id);
                        return true;
                    },
                    (foreignData) =>
                    {
                        cardFactory.ForeignByCard = foreignData.GroupBy(f => f.CardId).ToArray();
                        return true;
                    },
                    (legalities) =>
                    {
                        cardFactory.LegalitiesByCard = legalities.GroupBy(f => f.CardId).ToArray();
                        return true;
                    });

                return cardFactory.CreateCards();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}