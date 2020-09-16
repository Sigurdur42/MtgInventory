using System;
using ScryfallApi.Client.Models;

namespace MtgInventory.Service.Models
{
    // TODO: Müssen wir das ablachen?
    public class ScryfallCard
    {
        public ScryfallCard()
        {
        }

        public ScryfallCard(
            Card scryfallCard)
        {
            Card = scryfallCard;
            UpdateDateUtc = DateTime.UtcNow;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
       
        public string Set => Card?.Set;
      
        public string Name => Card?.Name;

        public Card Card { get; set; }

        public DateTime UpdateDateUtc { get; set; }
    }
}