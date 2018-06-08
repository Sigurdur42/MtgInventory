using System;

namespace MtgBinders.Domain.Services
{
    public interface IMtgCardService
    {
        int NumberOfCards { get; }
        event EventHandler InitializeDone;
        void Initialize();
    }
}