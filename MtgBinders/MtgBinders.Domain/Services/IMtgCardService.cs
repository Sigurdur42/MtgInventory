using MtgBinders.Domain.Entities;
using System;

namespace MtgBinders.Domain.Services
{
    public interface IMtgCardService
    {
        event EventHandler InitializeDone;

        int NumberOfCards { get; }

        void Initialize();

        void LoadMissingCardData(IMtgSetRepository setRepository);

        void LoadAllCardData();
    }
}