using System;
using System.Linq;
using MtgJson.Sqlite.Models;

namespace MtgJson.Sqlite.Repository
{
    public interface IMtgDataRepository : IDisposable
    {
        public IQueryable<DbCard>? MtgCards { get; }
        public IQueryable<DbSet>? MtgSets { get; }
    }
}