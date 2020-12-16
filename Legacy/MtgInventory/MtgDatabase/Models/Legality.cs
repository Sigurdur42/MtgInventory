namespace MtgDatabase.Models
{
    public class Legality
    {
        public SanctionedFormat Format { get; set; } = SanctionedFormat.Unknown;
        public LegalityState IsLegal { get; set; } = LegalityState.Unknown;
    }
}