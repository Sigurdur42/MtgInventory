using System.IO;
using Microsoft.EntityFrameworkCore;
using MtgJson.Sqlite.Models;

namespace MtgJson.Sqlite.Repository
{
    public class SqliteDatabaseContext : DbContext
    {
        private readonly FileInfo _sqliteName;

        public SqliteDatabaseContext(FileInfo sqliteName)
        {
            _sqliteName = sqliteName;
        }

        public DbSet<DbCard>? Cards { get; set; }
        public DbSet<DbSet>? Sets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_sqliteName.FullName};");
        }
    }
}