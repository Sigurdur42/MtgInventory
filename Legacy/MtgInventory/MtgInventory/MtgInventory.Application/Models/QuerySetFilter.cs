namespace MtgInventory.Service.Models
{
    public class QuerySetFilter
    {
        public string Name { get; set; } = "";
        public bool HideOnlyOneSide { get; set; }
        public bool HideKnownSets { get; set; }
    }
}