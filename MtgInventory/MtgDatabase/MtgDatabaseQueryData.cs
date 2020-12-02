namespace MtgDatabase
{
    public class MtgDatabaseQueryData
    {
        public string Name { get; set; } = "";
        
        public bool MatchExactName { get; set; }
        
        public bool IsToken { get; set; }

        public bool ContainsValidSearch()
        {
            return !string.IsNullOrWhiteSpace(Name)
                   || IsToken;
        }
    }
}