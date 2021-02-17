using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using MtgJson.Sqlite.Repository;
using Dapper;

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
            if (targetFile.Exists)
            {
                targetFile.Delete();
            }

            // https://dotnetcorecentral.com/blog/how-to-use-sqlite-with-dapper/

            using var connection = new DbConnection(targetFile);

            var exists = connection.QueryTableExists("cards");

            // TODO: continue here
        }

    }
}
