namespace MtgDatabase.Models
{
    public class Legality
    {
        public SanctionedFormat Format { get; set; } = SanctionedFormat.Unknown;
        public bool IsLegal { get; set; } = false;
    }
}