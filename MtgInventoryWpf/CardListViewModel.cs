using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        }

        public string PasteButtonToolTip => $"Paste a deck list from clipboard.{Environment.NewLine}Copy any deck list from Web sites or your favorite editor.";

        public DatabaseDeckReaderResult DeckReaderResult { get; private set; } = new DatabaseDeckReaderResult();

        internal async Task CopyFromClipboard()
        {
            if (!Clipboard.ContainsText())
            {
                _logger.LogWarning("Clipboard does not contain text - cannot import deck.");
                return;
            }

            var content = Clipboard.GetText();

            DeckReaderResult = await _mtgDatabaseService.ReadDeck(
                name: "Clipboard deck",
                deckContent: content);
        }
    }
}
