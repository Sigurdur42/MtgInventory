using System.Collections.ObjectModel;
using System.Linq;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    internal class MtgSetRepository : IMtgSetRepository
    {
        public MtgSetRepository()
        {
            SetData = new MtgSetInfo[0];
        }

        public int NumberOfSets { get; private set; }
        public MtgSetInfo[] SetData { get; private set; }

        public ReadOnlyDictionary<string, MtgSetInfo> SetDataByCode { get; private set; }

        public void SetSetData(MtgSetInfo[] setData)
        {
            NumberOfSets = setData.Length;
            SetData = setData;

            SetDataByCode = new ReadOnlyDictionary<string, MtgSetInfo>(SetData.ToDictionary(c=>c.SetCode.ToUpperInvariant()));
        }
    }
}