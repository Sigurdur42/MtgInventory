namespace MtgScryfall
{
    public interface IScryfallApi
    {
        RequestResult GetAllSets();

        RequestResult GetCardsByPage(int page);

        RequestResult GetCardsBySet(string setCode, int page);

        CardDataRequestResult GetCardsBySet(string setCode);
    }
}