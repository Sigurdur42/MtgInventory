using System.Diagnostics;

namespace MtgDomain
{
    [DebuggerDisplay("{Code} - {Name}")]
    public class SetInfo
    {
        // TODO: Implement ISerializable

        public string Name { get; set; }
        public string Code { get; set; }
        public int CardCount { get; set; }
        public SetReleaseDate ReleaseDate { get; set; }
        public bool IsDigital { get; set; }
    }
}