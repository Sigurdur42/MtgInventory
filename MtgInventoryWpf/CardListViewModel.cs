using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgDatabase.DatabaseDecks;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class CardListViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly ILogger<CardListViewModel> _logger;

        public CardListViewModel(
            IMtgDatabaseService mtgDatabaseService,
            ILogger<CardListViewModel> logger)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _logger = logger;

            UpdateDeckSummary();
        }

        public string PasteButtonToolTip => $"Paste a deck list from clipboard.{Environment.NewLine}Copy any deck list from Web sites or your favorite editor.";

        public DatabaseDeckReaderResult DeckReaderResult { get; private set; } = new DatabaseDeckReaderResult();

        public ObservableCollection<CardListViewLineViewModel> CardLines { get; } = new ObservableCollection<CardListViewLineViewModel>();

        public ListCollectionView? CardLineItems { get; private set; }

        public string CurrentDeckSummary { get; private set; } = "";

        internal async Task CopyFromClipboard()
        {
            if (!Clipboard.ContainsText())
            {
                _logger.LogWarning("Clipboard does not contain text - cannot import deck.");
                return;
            }

            var content = Clipboard.GetText();

            await ReadDeck(content);
        }

        public event EventHandler Reading = (sender, args) => { };
        public event EventHandler<TimeSpan> ReadingDone = (sender, args) => { };

        private async Task ReadDeck(string content)
        {
            var stopwatch = Stopwatch.StartNew();
            Reading?.Invoke(this, EventArgs.Empty);
            DeckReaderResult = await _mtgDatabaseService.ReadDeck(
                    name: "Clipboard deck",
                    deckContent: content);

            CardLines.Clear();
            foreach (var category in DeckReaderResult.Deck.Categories)
            {
                foreach (var line in category.Lines)
                {
                    CardLines.Add(new CardListViewLineViewModel(
                        _mtgDatabaseService,
                        line.Card,
                        line.Quantity)
                    {
                        Category = category.CategoryName,
                    });

                }
            }

            ListCollectionView collection = new ListCollectionView(CardLines);
            collection.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            CardLineItems = collection;

            UpdateDeckSummary();

            stopwatch.Stop();
            ReadingDone?.Invoke(this, stopwatch.Elapsed);
        }

        public void UpdateDeckSummary()
        {
            if (!CardLines.Any())
            {
                CurrentDeckSummary = "No deck loaded.";
            }

            var allCards = CardLines.Sum(c => c.Quantity);
            var totalPrice = CardLines.Where(c => c.PriceValue.HasValue).Sum(c => c.PriceValue);
            CurrentDeckSummary = $"{allCards} cards in deck: {totalPrice:f2} €";
        }
    }
}
