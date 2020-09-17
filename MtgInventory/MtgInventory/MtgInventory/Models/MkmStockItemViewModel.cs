using System;
using MkmApi;
using MtgInventory.Service;
using MtgInventory.Service.Models;
using ReactiveUI;

namespace MtgInventory.Models
{
    public class MkmStockItemViewModel : ReactiveObject
    {
        private CardPrice _cardPrice;
        private decimal? _marketPrice;

        public MkmStockItemViewModel(DetailedStockItem stockItem)
        {
            IdArticle = stockItem.IdArticle;
            IdProduct = stockItem.IdProduct;
            EnglishName = stockItem.EnglishName;
            SetCode = stockItem.SetCode;
            SetName = stockItem.SetName;
            Price = stockItem.Price;
            Language = stockItem.Language;
            Condition = stockItem.Condition;
            Foil = stockItem.Foil;
            Signed = stockItem.Signed;
            Playset = stockItem.Playset;
            Altered = stockItem.Altered;
            Comments = stockItem.Comments;
            Amount = stockItem.Amount;
            OnSale = stockItem.OnSale;
            ScryfallId = stockItem.ScryfallId;
        }

        public Guid ScryfallId { get; set; }

        public CardPrice CardPrice
        {
            get => _cardPrice;
            set
            {
                this.RaiseAndSetIfChanged(ref _cardPrice, value);
                MarketPrice = _cardPrice?.GetMarketPrice(Foil == "X");
            }
        }

        public decimal? MarketPrice
        {
            get => _marketPrice;
            set => this.RaiseAndSetIfChanged(ref _marketPrice, value);
        }

        public string IdArticle { get; set; }

        public string IdProduct { get; set; }

        public string EnglishName { get; set; }

        public string SetCode { get; set; }

        public string SetName { get; set; }

        public decimal Price { get; set; }

        public string Language { get; set; }

        public string Condition { get; set; }

        public string Foil { get; set; }

        public string Signed { get; set; }

        public string Playset { get; set; }

        public string Altered { get; set; }

        public string Comments { get; set; }

        public int Amount { get; set; }

        public bool OnSale { get; set; }
    }
}