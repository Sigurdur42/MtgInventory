using ScryfallApi.Client.Models;

namespace MtgBinder.Blazr.Data.CardLookup
{
    public class LookupResult
    {
        public LookupCard[] Cards { get; set; } = new LookupCard[0];
    }
}
