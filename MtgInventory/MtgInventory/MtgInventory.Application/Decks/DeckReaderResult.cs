namespace MtgInventory.Service.Decks
{
    public class DeckReaderResult
    {
        public DeckList Deck { get; set; } = new DeckList();

        public string[] UnreadLines { get; set; } = new string[0];
    }
}