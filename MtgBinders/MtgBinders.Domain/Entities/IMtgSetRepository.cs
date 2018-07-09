using System.Collections.ObjectModel;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    public interface IMtgSetRepository
    {
        int NumberOfSets { get; }

        MtgSetInfo[] SetData { get; }
        ReadOnlyDictionary<string, MtgSetInfo> SetDataByCode { get; }

        void SetSetData(MtgSetInfo[] setData);
    }
}