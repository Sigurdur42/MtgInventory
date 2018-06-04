namespace MtgScryfall
{
    public interface IScryfallApi
    {
        RequestResult GetAllSets();
        RequestResult GetCardsByPage(int page);
    }
}