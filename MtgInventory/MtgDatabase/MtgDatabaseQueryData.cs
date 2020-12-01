namespace MtgDatabase
{
    public class MtgDatabaseQueryData
    {
        public string Name { get; set; } = "";

        public bool ContainsValidSearch()
        {
            // TODO: Implement actual validation
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}