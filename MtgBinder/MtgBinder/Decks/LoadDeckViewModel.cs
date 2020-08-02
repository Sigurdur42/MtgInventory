using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MtgBinder.Domain.Decks;
using MtgBinder.Domain.Service;
using MtgBinder.Domain.Tools;
using MtgDomain;
using PropertyChanged;
using Serilog;

namespace MtgBinder.Decks
{
    [AddINotifyPropertyChangedInterface]
    public class LoadDeckViewModel
    {
        private readonly ICardService _cardService;
        private readonly IAsyncProgressNotifier _progressNotifier;

        public LoadDeckViewModel(
            ICardService cardService,
            IAsyncProgressNotifier progressNotifier)
        {
            _cardService = cardService;
            _progressNotifier = progressNotifier;
        }

        public DeckListViewModel CurrentDeck { get; set; }

        [AlsoNotifyFor(nameof(SelectedItem))]
        public DeckListItemViewModel SelectedItem { get; set; }

        public Uri SelectedCardUri => SelectedItem?.Card?.ImageUrls.FirstOrDefault(i => i.Key == "normal")?.Url ?? SelectedItem?.Card?.ImageUrls.FirstOrDefault()?.Url;

        public string UnreadableLines { get; set; }

        public void LoadDeckFromFile(FileInfo fileName)
        {
            try
            {
                var reader = new TextDeckReader();
                var result = reader.ReadDeck(File.ReadAllText(fileName.FullName), fileName.Name);

                UnreadableLines = string.Join(Environment.NewLine, result.UnreadLines);
                CurrentDeck = new DeckListViewModel(result.Deck, _cardService, _progressNotifier);
            }
            catch (Exception error)
            {
                Log.Error(error, $"Error loading deck from {fileName.FullName}: {error.Message}");
            }
        }
    }
}
