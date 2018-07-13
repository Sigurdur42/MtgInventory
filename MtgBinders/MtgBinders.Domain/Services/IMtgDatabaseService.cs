using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.Services
{
    public interface IMtgDatabaseService
    {
        event EventHandler Initialized;

        event EventHandler DatabaseUpdated;

        MtgSetInfo[] SetData { get; }
        MtgFullCard[] CardData { get; }

        int NumberOfCards { get; }
        int NumberOfSets { get; }

        bool IsInitialized { get; }

        DateTime? LastUpdated { get; }

        bool IsCardsMissing { get; }

        void Initialize();

        void UpdateDatabase(bool forceUpdate);

        void UpdateCardDetails(MtgFullCard card);
    }
}