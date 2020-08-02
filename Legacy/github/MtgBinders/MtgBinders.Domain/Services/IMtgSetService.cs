using MtgBinders.Domain.Entities;
using System;

namespace MtgBinders.Domain.Services
{
    public interface IMtgSetService
    {
        event EventHandler InitializeDone;

        DateTime? LastUpdatedCacheAt { get; }
        IMtgSetRepository SetRepository { get; }

        void Initialize();

        void UpdateSetsFromScryfall(bool checkLastUpdateDate);

        void WriteSetsToCache();
    }
}