using System;
using System.Linq;
using MtgJson.Sqlite.Models;

namespace MtgJson.Sqlite.Repository
{
    public class EmptyMtgDataRepository : IMtgDataRepository
    {
        public IQueryable<DbCard>? MtgCards { get; } = new EnumerableQuery<DbCard>(Array.Empty<DbCard>());

        public IQueryable<DbSet>? MtgSets { get; } = new EnumerableQuery<DbSet>(Array.Empty<DbSet>());

        public void Dispose()
        {
        }
    }
}