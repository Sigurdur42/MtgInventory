namespace MtgInventory.Service.Decks
{
    public class DeckReaderResult
    {
        public DeckList Deck { get; set; }

        public string[] UnreadLines { get; set; }
    }
}