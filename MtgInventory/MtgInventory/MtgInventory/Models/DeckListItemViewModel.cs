using System;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Models;
using ReactiveUI;

namespace MtgInventory.Models
{
    public class DeckListItemViewModel : ReactiveObject
    {
        private string _name = "";
        private string _setCode = "";
        private string _setName = "";
        private int _quantity;
        private Guid _cardId;

        private decimal _price;

        private CardPriceSource _priceSource;

        public DeckListItemViewModel(DeckItem item)
        {
            Name = item.Name;
            Quantity = item.Count;
            SetCode = item.SetCode;
            SetName = item.SetName;
        }

        public Guid CardId
        {
            get => _cardId;
            set => this.RaiseAndSetIfChanged(ref _cardId, value);
        }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string SetName
        {
            get => _setName;
            set => this.RaiseAndSetIfChanged(ref _setName, value);
        }

        public string SetCode
        {
            get => _setCode;
            set => this.RaiseAndSetIfChanged(ref _setCode, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => this.RaiseAndSetIfChanged(ref _quantity, value);
        }

        public decimal Price
        {
            get => _price;
            set => this.RaiseAndSetIfChanged(ref _price, value);
        }

        public CardPriceSource PriceSource
        {
            get => _priceSource;
            set => this.RaiseAndSetIfChanged(ref _priceSource, value);
        }
    }
}