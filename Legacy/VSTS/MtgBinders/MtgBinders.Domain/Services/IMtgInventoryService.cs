using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace MtgBinders.Domain.Services
{
    public interface IMtgInventoryService
    {
        event EventHandler Initialized;

        bool IsInitialized { get; }

        IList<MtgInventoryCard> Cards { get; }

        void Initialize();

        void SaveInventory();
    }
}