namespace MtgScryfall
{
    public interface IScryfallApi
    {
        RequestResult GetAllSets();

        RequestResult GetCardsByPageJson(int page);

        CardDataRequestResult GetCardsByPage(int page);

        RequestResult GetCardsBySet(string setCode, int page);

        CardDataRequestResult GetCardsBySet(string setCode);
    }
}