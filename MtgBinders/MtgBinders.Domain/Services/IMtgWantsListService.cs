using System.Collections.Generic;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Services
{
    public interface IMtgWantsListService
    {
        IEnumerable<WantListCard> Wants { get; }

        void AddWant(MtgFullCard card, int count);

        void Initialize();
    }
}