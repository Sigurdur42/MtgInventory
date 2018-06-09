using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    public interface IMtgSetRepository
    {
        int NumberOfSets { get; }

        MtgSetInfo[] SetData { get; }

        void SetSetData(MtgSetInfo[] setData);
    }
}