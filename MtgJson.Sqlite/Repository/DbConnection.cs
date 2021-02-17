using System;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;

namespace MtgJson.Sqlite.Repository
{
    internal sealed class DbConnection : IDisposable
    {
        public DbConnection(FileInfo databaseFile)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = databaseFile.FullName,
            };

            Connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
        }

        public SqliteConnection Connection { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public bool QueryTableExists(string tableName)
            => Connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';")
                        ?.Any() ?? false;
    }
}