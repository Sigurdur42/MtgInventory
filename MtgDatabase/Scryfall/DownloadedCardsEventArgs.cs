using System;
using System.Collections.Generic;

namespace MtgDatabase.Scryfall
{
    public class DownloadedCardsEventArgs : EventArgs
    {
        public DownloadedCardsEventArgs(IEnumerable<ScryfallJsonCard> downloadedCards)
        {
            DownloadedCards = downloadedCards;
        }

        public IEnumerable<ScryfallJsonCard> DownloadedCards { get; }
    }
}