namespace MtgDatabase.Decks
{
    public class DeckReaderResult
    {
        public string Name { get; set; } = "";
        public Deck Deck { get; set; } = new Deck();

        public string[] UnreadLines { get; set; } = new string[0];
    }
}