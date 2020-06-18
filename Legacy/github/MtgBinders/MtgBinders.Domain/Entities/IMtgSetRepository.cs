using System;
using System.Collections.ObjectModel;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    public interface IMtgSetRepository
    {
        event EventHandler SetDataUpdated;

        int NumberOfSets { get; }

        MtgSetInfo[] SetData { get; }
        ReadOnlyDictionary<string, MtgSetInfo> SetDataByCode { get; }

        void SetSetData(MtgSetInfo[] setData);
    }
}