using System.Collections.Generic;
using MtgBinders.Domain.Entities;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Services
{
    public interface IMtgWantsListService
    {
        IEnumerable<MtgWantListCard> Wants { get; }

        MtgWantListCard AddWant(MtgFullCard card, int count);

        void Initialize();
    }
}