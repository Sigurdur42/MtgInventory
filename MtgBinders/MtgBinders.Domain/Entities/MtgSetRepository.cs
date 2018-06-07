using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    internal class MtgSetRepository : IMtgSetRepository
    {
        public int NumberOfSets { get; private set; }
        public MtgSetInfo[] SetData { get; private set; }

        internal void SetSetData(MtgSetInfo[] setData)
        {
            NumberOfSets = setData.Length;
            SetData = setData;
        }
    }
}