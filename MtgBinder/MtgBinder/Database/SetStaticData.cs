using System.Diagnostics;

namespace MtgBinder.Database
{
    [DebuggerDisplay("{SetCode} - {SetName}")]
    public class SetStaticData
    {
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public int CardsPerSet { get; set; }
        public int DownloadedCards { get; set; }
    }
}