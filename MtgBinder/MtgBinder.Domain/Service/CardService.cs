using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Scryfall;
using MtgBinder.Domain.Tools;
using MtgDomain;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Service
{
    public interface ICardService
    {
        CardInfo LookupCard(string name);
    }

    public class CardService : ICardService
    {
        private readonly ICardDatabase _database;
        private readonly IScryfallService _scryfallService;

        public CardService(ICardDatabase database, IScryfallService scryfallService)
        {
            _database = database;
            _scryfallService = scryfallService;
        }

        public CardInfo LookupCard(string name)
        {
            using var logger = new ActionLogger(nameof(CardService), nameof(LookupCard) + $"({name})");
            
            var info = _database.Cards
                .FindOne(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (info == null)
            {
                // Need to grab this card from scryfall first
                logger.Information("need to grab card from scryfall...");
                info = _database.LookupCards(name, SearchOptions.RollupMode.Prints).FirstOrDefault();
            }

            return info;
        }
    }
}
