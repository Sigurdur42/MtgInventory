namespace MtgDatabase
{
    public enum ResultSortOrder
    {
        ByName = 0,
        ByCollectorNumber = 1,
    }

    public class MtgDatabaseQueryData
    {
        public string Name { get; set; } = "";

        public string SetCode { get; set; } = "";

        public bool MatchExactName { get; set; }

        public bool IsToken { get; set; }

        public bool ResultPerPrinting { get; set; }

        public bool FilterBySet => !string.IsNullOrWhiteSpace(SetCode);

        public ResultSortOrder ResultSortOrder { get; set; } = ResultSortOrder.ByName;

        public bool ContainsValidSearch()
        {
            return !string.IsNullOrWhiteSpace(Name)
                   || !string.IsNullOrWhiteSpace(SetCode)
                   || IsToken;
        }
    }
}