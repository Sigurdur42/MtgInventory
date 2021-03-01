using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MtgJson.Sqlite.Models;

namespace MtgJson.Sqlite.Repository
{
    public class SqliteDatabaseContext : DbContext, IMtgDataRepository
    {
        private readonly FileInfo _sqliteName;

        public SqliteDatabaseContext(FileInfo sqliteName)
        {
            _sqliteName = sqliteName;
        }

        public DbSet<DbCard>? Cards { get; set; }
        public IQueryable<DbCard>? MtgCards => Cards;
        public IQueryable<DbSet>? MtgSets => Sets;

        public DbSet<DbSet>? Sets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_sqliteName.FullName};");
        }
    }
}