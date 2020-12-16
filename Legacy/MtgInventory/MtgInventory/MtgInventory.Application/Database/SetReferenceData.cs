using System.Diagnostics;

namespace MtgInventory.Service.Database
{
    [DebuggerDisplay("{MkmCode}->{ScryfallCode}")]
    public class SetReferenceData
    {
        public string MkmCode { get; set; } = "";
        public string ScryfallCode { get; set; } = "";
    }
}