using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Services
{
    public interface ICardSearchService
    {
        MtgFullCard[] Search(string searchPattern, CardSearchSettings settings);
    }
}